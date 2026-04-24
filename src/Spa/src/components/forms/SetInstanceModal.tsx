import {useState} from 'react';
import {Modal} from '../Modal';
import {Button} from '../Button';
import {BooleanInstanceForm} from './BooleanInstanceForm';
import {SatInstanceForm} from './SatInstanceForm';
import {CspInstanceForm} from './CspInstanceForm';
import {GraphEditorForm} from './GraphEditorForm';
import {INSTANCE_TYPES, type InstanceTypeValue} from '../../lib/constants';
import {api} from '../../lib/api';
import type {CspInstanceDto, VertexColoringEdgeDto,} from '../../services/combinatorics';

interface SetInstanceModalProps {
  open: boolean;
  problemId: string;
  onClose: () => void;
  onSuccess: () => void;
}

export function SetInstanceModal({
  open,
  problemId,
  onClose,
  onSuccess,
}: SetInstanceModalProps) {
  const [selectedType, setSelectedType] = useState<InstanceTypeValue | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleClose = () => {
    setSelectedType(null);
    setError(null);
    onClose();
  };

  const wrap = async (fn: () => Promise<void>) => {
    setLoading(true);
    setError(null);
    try {
      await fn();
      handleClose();
      onSuccess();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to set instance');
    } finally {
      setLoading(false);
    }
  };

  const renderForm = () => {
    switch (selectedType) {
      case 'bool':
        return (
          <BooleanInstanceForm
            loading={loading}
            onSubmit={(formula) =>
              wrap(() => api.setBooleanInstance(problemId, { instance: { formula } }))
            }
          />
        );
      case 'sat':
        return (
          <SatInstanceForm
            loading={loading}
            onSubmit={(clauses) =>
              wrap(() => api.setSatInstance(problemId, { instance: { clauses } }))
            }
          />
        );
      case 'csp':
        return (
          <CspInstanceForm
            loading={loading}
            onSubmit={(instance: CspInstanceDto) =>
              wrap(() => api.setCspInstance(problemId, { instance }))
            }
          />
        );
      case 'vertex-coloring':
        return (
          <GraphEditorForm
            loading={loading}
            onSubmit={(verts, edgs, colorCount) =>
              wrap(() =>
                api.setVertexColoringInstance(problemId, {
                  instance: {
                    vertices: verts,
                    edges: edgs as VertexColoringEdgeDto[],
                    colorCount,
                  },
                }),
              )
            }
          />
        );
      case 'edge-coloring':
        return (
          <GraphEditorForm
            loading={loading}
            onSubmit={(verts, edgs, colorCount) =>
              wrap(() =>
                api.setEdgeColoringInstance(problemId, {
                  instance: {
                    vertices: verts,
                    edges: edgs,
                    colorCount,
                  },
                }),
              )
            }
          />
        );
      default:
        return null;
    }
  };

  return (
    <Modal
      open={open}
      title={selectedType ? `Set Instance — ${selectedType}` : 'Set Problem Instance'}
      onClose={handleClose}
    >
      {!selectedType ? (
        <div className="space-y-2">
          <p className="text-xs mb-3" style={{ color: '#545b64' }}>Choose a problem type:</p>
          {INSTANCE_TYPES.map((t) => (
            <button
              key={t.value}
              onClick={() => setSelectedType(t.value)}
              className="w-full text-left px-3 py-2.5 rounded border transition-colors cursor-pointer"
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
      ) : (
        <div>
          <Button
            size="sm"
            variant="ghost"
            onClick={() => setSelectedType(null)}
            className="mb-4"
          >
            ← Back
          </Button>
          {renderForm()}
          {error && (
            <p className="text-xs mt-3" style={{ color: '#d13212' }}>
              {error}
            </p>
          )}
        </div>
      )}
    </Modal>
  );
}
