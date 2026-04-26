import {useState} from 'react';
import {Modal} from '../Modal';
import {Button} from '../Button';
import {BooleanInstanceForm} from './BooleanInstanceForm';
import {SatInstanceForm} from './SatInstanceForm';
import {CspInstanceForm} from './CspInstanceForm';
import {circleLayout, GraphEditorForm} from './GraphEditorForm';
import {INSTANCE_TYPES, type InstanceTypeValue} from '../../lib/constants';
import {api} from '../../lib/api';
import type {AnyInstanceData} from '../../hooks/useInstance';
import type {
    BooleanProblemInstanceDto,
    CspInstanceDto,
    EdgeColoringInstanceDto,
    SatInstanceDto,
    VertexColoringInstanceDto,
} from '../../services/combinatorics';

// ─── Version name helper ──────────────────────────────────────────────────────

/** Increment "(version N)" suffix, or append "(version 2)" if absent. */
export function nextVersionName(name: string): string {
  const match = name.match(/^(.*?)\s*\(version\s+(\d+)\)\s*$/i);
  if (match) {
    const base = (match[1] ?? '').trim();
    const n = parseInt(match[2] ?? '1', 10);
    return `${base} (version ${n + 1})`;
  }
  return `${name} (version 2)`;
}

// ─── Props ────────────────────────────────────────────────────────────────────

export interface ForkPrefill {
  name: string;
  description: string;
  instanceType: InstanceTypeValue;
  instance: AnyInstanceData;
}

interface CreateProblemModalProps {
  open: boolean;
  onClose: () => void;
  /** Called with the new problem's ID after creation. */
  onSuccess: (newProblemId: string) => void;
  /** When provided, skips type picker and pre-fills all fields (fork/edit mode). */
  prefill?: ForkPrefill;
}

// ─── Component ───────────────────────────────────────────────────────────────

export function SetInstanceModal({
  open,
  onClose,
  onSuccess,
  prefill,
}: CreateProblemModalProps) {
  const isFork = prefill !== undefined;

  const [selectedType, setSelectedType] = useState<InstanceTypeValue | null>(
    prefill?.instanceType ?? null,
  );
  const [name, setName] = useState(isFork ? nextVersionName(prefill!.name) : '');
  const [description, setDescription] = useState(isFork ? (prefill!.description ?? '') : '');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleClose = () => {
    if (!isFork) {
      setSelectedType(null);
    }
    setName(isFork ? nextVersionName(prefill!.name) : '');
    setDescription(isFork ? (prefill!.description ?? '') : '');
    setError(null);
    onClose();
  };

  const wrap = async (fn: () => Promise<string>) => {
    if (!name.trim()) {
      setError('Name is required');
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const id = await fn();
      handleClose();
      onSuccess(id);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create problem');
    } finally {
      setLoading(false);
    }
  };

  // ── Per-type submit handlers ─────────────────────────────────────────────

  const handleBoolSubmit = async (formula: string) => {
    await wrap(async () => {
      const res = await api.createBooleanProblem({
        name: name.trim(),
        description: description || null,
        instance: { formula },
      });
      return res.problemId;
    });
  };

  const handleSatSubmit = async (clauses: string[][]) => {
    await wrap(async () => {
      const res = await api.createSatProblem({
        name: name.trim(),
        description: description || null,
        instance: { clauses },
      });
      return res.problemId;
    });
  };

  const handleCspSubmit = async (instance: CspInstanceDto) => {
    await wrap(async () => {
      const res = await api.createCspProblem({
        name: name.trim(),
        description: description || null,
        instance,
      });
      return res.problemId;
    });
  };

  // ── Build pre-fill data for forms ────────────────────────────────────────

  const getPrefillProps = () => {
    if (!prefill) return {};
    switch (prefill.instanceType) {
      case 'bool': {
        const d = prefill.instance as BooleanProblemInstanceDto;
        return { initialFormula: d.formula };
      }
      case 'sat': {
        const d = prefill.instance as SatInstanceDto;
        return { initialText: d.clauses.map((c: string[]) => c.join(' ')).join('\n') };
      }
      case 'csp': {
        const d = prefill.instance as CspInstanceDto;
        return {
          initialVariables: d.variables.map((v) => ({
            name: v.name,
            states: v.states.join(', '),
          })),
          initialConstraints: d.constraints.length ? d.constraints : [''],
        };
      }
      case 'vertex-coloring':
      case 'edge-coloring': {
        const d = prefill.instance as VertexColoringInstanceDto | EdgeColoringInstanceDto;
        return {
          initialVertices: circleLayout(d.graph.vertices.map((v) => v.id)),
          initialEdges: d.graph.edges,
          initialColorCount: d.colorCount,
        };
      }
      default:
        return {};
    }
  };

  const prefillProps = getPrefillProps();

  // ── Render form for selected type ────────────────────────────────────────

  const renderForm = () => {
    switch (selectedType) {
      case 'bool':
        return (
          <BooleanInstanceForm
            loading={loading}
            onSubmit={handleBoolSubmit}
            {...(prefillProps as { initialFormula?: string })}
          />
        );
      case 'sat':
        return (
          <SatInstanceForm
            loading={loading}
            onSubmit={handleSatSubmit}
            {...(prefillProps as { initialText?: string })}
          />
        );
      case 'csp':
        return (
          <CspInstanceForm
            loading={loading}
            onSubmit={handleCspSubmit}
            {...(prefillProps as object)}
          />
        );
      case 'vertex-coloring':
        return (
          <GraphEditorForm
            loading={loading}
            onSubmit={async (inst) => {
              await wrap(async () => {
                const res = await api.createVertexColoringProblem({
                  name: name.trim(),
                  description: description || null,
                  instance: inst,
                });
                return res.problemId;
              });
            }}
            {...(prefillProps as object)}
          />
        );
      case 'edge-coloring':
        return (
          <GraphEditorForm
            loading={loading}
            onSubmit={async (inst) => {
              await wrap(async () => {
                const res = await api.createEdgeColoringProblem({
                  name: name.trim(),
                  description: description || null,
                  instance: inst,
                });
                return res.problemId;
              });
            }}
            {...(prefillProps as object)}
          />
        );
      default:
        return null;
    }
  };

  const modalTitle = isFork
    ? 'Edit problem (new version)'
    : selectedType
      ? `New problem — ${INSTANCE_TYPES.find((t) => t.value === selectedType)?.label ?? selectedType}`
      : 'New problem';

  return (
    <Modal open={open} title={modalTitle} onClose={handleClose}>
      <div className="space-y-4">
        {/* Name + description always visible */}
        <div>
          <label className="block text-xs font-semibold mb-1" style={{ color: '#16191f' }}>
            Name <span style={{ color: '#d13212' }}>*</span>
          </label>
          <input
            className="w-full border rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-1"
            style={{ borderColor: '#aab7b8', color: '#16191f' }}
            value={name}
            onChange={(e) => setName(e.target.value)}
            disabled={loading}
            placeholder="Problem name"
          />
        </div>
        <div>
          <label className="block text-xs font-semibold mb-1" style={{ color: '#16191f' }}>
            Description
          </label>
          <textarea
            className="w-full border rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-1"
            style={{ borderColor: '#aab7b8', color: '#16191f' }}
            rows={2}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            disabled={loading}
            placeholder="Optional description…"
          />
        </div>

        {/* Type picker (create mode only) */}
        {!isFork && !selectedType && (
          <div>
            <p className="text-xs mb-2" style={{ color: '#545b64' }}>Choose problem type:</p>
            {INSTANCE_TYPES.map((t) => (
              <button
                key={t.value}
                onClick={() => setSelectedType(t.value)}
                className="w-full text-left px-3 py-2.5 rounded border mb-1.5 transition-colors cursor-pointer"
                style={{ borderColor: '#d5dbdb', background: '#fff', color: '#16191f' }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.background = '#fef6e4';
                  e.currentTarget.style.borderColor = '#ff9900';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.background = '#fff';
                  e.currentTarget.style.borderColor = '#d5dbdb';
                }}
              >
                <span className="text-sm font-medium">{t.label}</span>
              </button>
            ))}
          </div>
        )}

        {/* Back button (create mode, type selected) */}
        {!isFork && selectedType && (
          <Button size="sm" variant="ghost" onClick={() => setSelectedType(null)}>
            ← Back
          </Button>
        )}

        {/* Instance form */}
        {selectedType && renderForm()}

        {error && (
          <p className="text-xs" style={{ color: '#d13212' }}>
            {error}
          </p>
        )}
      </div>
    </Modal>
  );
}

// Re-export under old name for backward compat with App.tsx import path
export { SetInstanceModal as CreateProblemModal };
