import {Spinner} from '../Spinner';
import {GraphCanvas} from '../GraphCanvas';
import type {AnyInstanceData} from '../../hooks/useInstance';
import type {
    BooleanProblemInstanceDto,
    CspInstanceDto,
    EdgeColoringInstanceDto,
    SatInstanceDto,
    VertexColoringInstanceDto,
} from '../../services/combinatorics';

// ─── Per-type renderers ───────────────────────────────────────────────────────

function BooleanInstance({ data }: { data: BooleanProblemInstanceDto }) {
  return (
    <div>
      <p className="text-xs font-medium mb-1" style={{ color: '#545b64' }}>Formula</p>
      <pre
        className="text-sm font-mono rounded px-4 py-3 whitespace-pre-wrap break-all border"
        style={{ background: '#f2f3f3', borderColor: '#d5dbdb', color: '#16191f' }}
      >
        {data.formula}
      </pre>
    </div>
  );
}

function SatInstance({ data }: { data: SatInstanceDto }) {
  return (
    <div>
      <p className="text-xs font-medium mb-1" style={{ color: '#545b64' }}>
        Clauses ({data.clauses.length})
      </p>
      <div className="overflow-auto max-h-56 border rounded" style={{ borderColor: '#d5dbdb' }}>
        <table className="w-full text-xs font-mono">
          <thead className="sticky top-0" style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
            <tr>
              <th className="text-left px-3 py-2 font-medium w-12" style={{ color: '#545b64' }}>#</th>
              <th className="text-left px-3 py-2 font-medium" style={{ color: '#545b64' }}>Literals</th>
            </tr>
          </thead>
          <tbody>
            {data.clauses.map((clause: string[], i: number) => (
              <tr key={i} style={{ borderTop: '1px solid #eaeded' }}>
                <td className="px-3 py-1" style={{ color: '#879596' }}>{i + 1}</td>
                <td className="px-3 py-1" style={{ color: '#16191f' }}>{clause.join('  ')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function CspInstance({ data }: { data: CspInstanceDto }) {
  return (
    <div className="space-y-4">
      <div>
        <p className="text-xs font-medium mb-1" style={{ color: '#545b64' }}>
          Variables ({data.variables.length})
        </p>
        <div className="overflow-auto max-h-48 border rounded" style={{ borderColor: '#d5dbdb' }}>
          <table className="w-full text-sm">
            <thead className="sticky top-0" style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
              <tr>
                <th className="text-left px-3 py-2 text-xs font-medium" style={{ color: '#545b64' }}>Name</th>
                <th className="text-left px-3 py-2 text-xs font-medium" style={{ color: '#545b64' }}>States</th>
              </tr>
            </thead>
            <tbody>
              {data.variables.map((v) => (
                <tr key={v.name} style={{ borderTop: '1px solid #eaeded' }}>
                  <td className="px-3 py-1.5 font-mono text-xs" style={{ color: '#16191f' }}>{v.name}</td>
                  <td className="px-3 py-1.5 text-xs">
                    {v.states.map((s, i) => (
                      <span
                        key={i}
                        className="inline-block mr-1 mb-0.5 px-1.5 py-0.5 rounded text-xs"
                        style={{ background: '#fef6e4', color: '#16191f', border: '1px solid #f5a623' }}
                      >
                        {s}
                      </span>
                    ))}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {data.constraints.length > 0 && (
        <div>
          <p className="text-xs font-medium mb-1" style={{ color: '#545b64' }}>
            Constraints ({data.constraints.length})
          </p>
          <ul className="space-y-1">
            {data.constraints.map((c, i) => (
              <li
                key={i}
                className="text-xs font-mono rounded px-3 py-1.5 border"
                style={{ background: '#f2f3f3', borderColor: '#d5dbdb', color: '#16191f' }}
              >
                {c}
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}

function GraphInstance({
  data,
  label,
}: {
  data: VertexColoringInstanceDto | EdgeColoringInstanceDto;
  label: string;
}) {
  const { graph, colorCount } = data;

  return (
    <div className="space-y-3">
      <div className="flex gap-8 text-sm">
        <span><span style={{ color: '#545b64' }}>Vertices: </span><strong>{graph.vertices.length}</strong></span>
        <span><span style={{ color: '#545b64' }}>Edges: </span><strong>{graph.edges.length}</strong></span>
        <span><span style={{ color: '#545b64' }}>Colors ({label}): </span><strong>{colorCount}</strong></span>
      </div>
      <p className="text-xs" style={{ color: '#879596' }}>
        Drag vertices to rearrange layout. Scroll to zoom.
      </p>
      <GraphCanvas
        vertices={graph.vertices}
        edges={graph.edges}
        movable
        height={300}
      />
    </div>
  );
}

// ─── Main section ─────────────────────────────────────────────────────────────

interface InstanceSectionProps {
  instanceType: string | null;
  instance: AnyInstanceData | null;
  loading: boolean;
  error: string | null;
}

export function InstanceSection({ instanceType, instance, loading, error }: InstanceSectionProps) {
  if (!instanceType) return null;

  return (
    <section className="bg-white border rounded" style={{ borderColor: '#d5dbdb' }}>
      <div
        className="px-4 py-3 border-b flex items-center gap-3"
        style={{ borderColor: '#d5dbdb', background: '#fafafa' }}
      >
        <h2 className="text-sm font-semibold" style={{ color: '#16191f' }}>
          Problem instance
        </h2>
        <span
          className="text-xs px-2 py-0.5 rounded font-medium"
          style={{ background: '#fef6e4', color: '#16191f', border: '1px solid #f5a623' }}
        >
          {instanceType}
        </span>
      </div>

      <div className="px-4 py-4">
        {loading && (
          <div className="flex items-center gap-2 text-sm" style={{ color: '#545b64' }}>
            <Spinner size="sm" /> Loading instance…
          </div>
        )}
        {error && <p className="text-sm" style={{ color: '#d13212' }}>{error}</p>}

        {instance && (
          <>
            {instanceType === 'bool' && <BooleanInstance data={instance as BooleanProblemInstanceDto} />}
            {instanceType === 'sat' && <SatInstance data={instance as SatInstanceDto} />}
            {instanceType === 'csp' && <CspInstance data={instance as CspInstanceDto} />}
            {instanceType === 'vertex-coloring' && (
              <GraphInstance data={instance as VertexColoringInstanceDto} label="vertex" />
            )}
            {instanceType === 'edge-coloring' && (
              <GraphInstance data={instance as EdgeColoringInstanceDto} label="edge" />
            )}
          </>
        )}
      </div>
    </section>
  );
}
