import {useState} from 'react';
import {useLocation, useNavigate} from 'react-router-dom';
import {api} from '../lib/api';
import {INSTANCE_TYPES, type InstanceTypeValue} from '../lib/constants';
import {circleLayout, GraphEditorForm} from '../components/forms/GraphEditorForm';
import {BooleanInstanceForm} from '../components/forms/BooleanInstanceForm';
import {SatInstanceForm} from '../components/forms/SatInstanceForm';
import {CspInstanceForm} from '../components/forms/CspInstanceForm';
import {ErrorStack, useErrorStack} from '../components/ErrorStack';
import type {AnyInstanceData} from '../hooks/useInstance';
import type {
  BooleanProblemInstanceDto,
  CspInstanceDto,
  EdgeColoringInstanceDto,
  SatInstanceDto,
  VertexColoringInstanceDto,
} from '../services/combinatorics';

// ─── Fork prefill (passed via router state) ───────────────────────────────────

export interface ForkPrefill {
  name: string;
  description: string;
  instanceType: InstanceTypeValue;
  instance: AnyInstanceData;
}

export function nextVersionName(name: string): string {
  const match = name.match(/^(.*?)\s*\(version\s+(\d+)\)\s*$/i);
  if (match) {
    const base = (match[1] ?? '').trim();
    const n = parseInt(match[2] ?? '1', 10);
    return `${base} (version ${n + 1})`;
  }
  return `${name} (version 2)`;
}

// ─── Component ────────────────────────────────────────────────────────────────

interface CreateProblemPageProps {
  onProblemChanged: () => void;
}

export function CreateProblemPage({ onProblemChanged }: CreateProblemPageProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const prefill = (location.state as { fork?: ForkPrefill } | null)?.fork;
  const isFork = prefill !== undefined;

  const [selectedType, setSelectedType] = useState<InstanceTypeValue | null>(
    prefill?.instanceType ?? null,
  );
  const [name, setName] = useState(isFork ? nextVersionName(prefill!.name) : '');
  const [description, setDescription] = useState(isFork ? (prefill!.description ?? '') : '');
  const [loading, setLoading] = useState(false);

  const { errors, addError, dismiss } = useErrorStack();

  // ── Submit wrapper ────────────────────────────────────────────────────────

  const wrap = async (fn: () => Promise<string>) => {
    if (!name.trim()) {
      addError('Name is required');
      return;
    }
    setLoading(true);
    try {
      const id = await fn();
      await onProblemChanged();
      navigate(`/problems/${id}`);
    } catch (err) {
      addError(err instanceof Error ? err.message : 'Failed to create problem');
    } finally {
      setLoading(false);
    }
  };

  // ── Per-type submit handlers ──────────────────────────────────────────────

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

  const handleVertexColoringSubmit = async (inst: { graph: import('../services/combinatorics').GraphDto; colorCount: number }) => {
    await wrap(async () => {
      const res = await api.createVertexColoringProblem({
        name: name.trim(),
        description: description || null,
        instance: inst,
      });
      return res.problemId;
    });
  };

  const handleEdgeColoringSubmit = async (inst: { graph: import('../services/combinatorics').GraphDto; colorCount: number }) => {
    await wrap(async () => {
      const res = await api.createEdgeColoringProblem({
        name: name.trim(),
        description: description || null,
        instance: inst,
      });
      return res.problemId;
    });
  };

  // ── Build pre-fill props for instance forms ───────────────────────────────

  const getPrefillProps = (): Record<string, unknown> => {
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
          initialVariables: d.variables.map((v) => ({ name: v.name, states: v.states.join(', ') })),
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

  // ── Instance form for selected type ──────────────────────────────────────

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
            onSubmit={handleVertexColoringSubmit}
            {...(prefillProps as object)}
          />
        );
      case 'edge-coloring':
        return (
          <GraphEditorForm
            loading={loading}
            onSubmit={handleEdgeColoringSubmit}
            {...(prefillProps as object)}
          />
        );
      default:
        return null;
    }
  };

  const pageTitle = isFork
    ? 'New version'
    : selectedType
      ? `New problem — ${INSTANCE_TYPES.find((t) => t.value === selectedType)?.label ?? selectedType}`
      : 'New problem';

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <div className="flex flex-col h-full">
      {/* Page header */}
      <div
        className="shrink-0 border-b px-6 py-4"
        style={{ background: '#ffffff', borderColor: '#d5dbdb' }}
      >
        <p className="text-xs mb-1" style={{ color: '#879596' }}>
          Problems <span className="mx-1">›</span>
          <span style={{ color: '#0073bb' }}>
            {isFork ? `${prefill!.name} › New version` : 'New problem'}
          </span>
        </p>
        <div className="flex items-center justify-between">
          <h1 className="text-lg font-semibold" style={{ color: '#16191f' }}>
            {pageTitle}
          </h1>
          <button
            onClick={() => navigate(-1)}
            className="rounded border px-3 py-1.5 text-sm cursor-pointer"
            style={{ borderColor: '#aab7b8', color: '#545b64', background: '#fff' }}
          >
            Cancel
          </button>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto px-6 py-5">
        <div className="w-full space-y-5">
          {/* Errors */}
          <ErrorStack errors={errors} onDismiss={dismiss} />

          {/* Name */}
          <div>
            <label className="block text-xs font-semibold mb-1" style={{ color: '#16191f' }}>
              Name <span style={{ color: '#d13212' }}>*</span>
            </label>
            <input
              className="w-full rounded border px-3 py-2 text-sm focus:outline-none focus:ring-1"
              style={{ borderColor: '#aab7b8', color: '#16191f' }}
              value={name}
              onChange={(e) => setName(e.target.value)}
              disabled={loading}
              placeholder="Problem name"
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-xs font-semibold mb-1" style={{ color: '#16191f' }}>
              Description
            </label>
            <textarea
              className="w-full rounded border px-3 py-2 text-sm focus:outline-none focus:ring-1"
              style={{ borderColor: '#aab7b8', color: '#16191f' }}
              rows={3}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              disabled={loading}
              placeholder="Optional description…"
            />
          </div>

          {/* Type picker — create mode only, before type is selected */}
          {!isFork && !selectedType && (
            <div>
              <p className="text-xs font-semibold mb-2" style={{ color: '#16191f' }}>
                Choose problem type:
              </p>
              <div className="space-y-1.5">
                {INSTANCE_TYPES.map((t) => (
                  <button
                    key={t.value}
                    onClick={() => setSelectedType(t.value)}
                    className="w-full rounded border px-4 py-3 text-left transition-colors cursor-pointer"
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
            </div>
          )}

          {/* Back button — create mode, after type selected */}
          {!isFork && selectedType && (
            <button
              onClick={() => setSelectedType(null)}
              className="text-sm cursor-pointer hover:underline"
              style={{ color: '#0073bb' }}
            >
              ← Back to type selection
            </button>
          )}

          {/* Instance form */}
          {selectedType && renderForm()}
        </div>
      </div>
    </div>
  );
}
