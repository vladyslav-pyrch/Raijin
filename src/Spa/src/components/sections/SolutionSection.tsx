import {useSolution} from '../../hooks/useSolution';
import {Spinner} from '../Spinner';
import type {
    GetBooleanSatisfiabilitySolutionResponse,
    GetBooleanSolutionResponse,
    GetCspSolutionResponse,
    GetEdgeColoringSolutionResponse,
    GetVertexColoringSolutionResponse,
} from '../../services/combinatorics';

function SatisfiabilityChip({ value }: { value: string | number }) {
  const str = String(value);
  const isSat = str === 'Satisfiable' || str === '1';
  const isUnsat = str === 'Unsatisfiable' || str === '2';
  const label = str === '1' ? 'Satisfiable' : str === '2' ? 'Unsatisfiable' : str === '0' ? 'Unknown' : str;
  const style = isSat
    ? { background: '#d5f5e3', color: '#1d8348', border: '1px solid #1d8348' }
    : isUnsat
      ? { background: '#fadbd8', color: '#d13212', border: '1px solid #d13212' }
      : { background: '#f2f3f3', color: '#545b64', border: '1px solid #aab7b8' };
  return (
    <span className="inline-block px-2.5 py-0.5 rounded text-xs font-semibold" style={style}>
      {label}
    </span>
  );
}

function BoolTable({ assignments }: { assignments: { variableName: string; value: boolean }[] }) {
  return (
    <table className="w-full text-sm border rounded overflow-hidden" style={{ borderColor: '#d5dbdb' }}>
      <thead style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
        <tr>
          <th className="text-left px-3 py-2 text-xs font-medium" style={{ color: '#545b64' }}>Variable</th>
          <th className="text-left px-3 py-2 text-xs font-medium" style={{ color: '#545b64' }}>Value</th>
        </tr>
      </thead>
      <tbody>
        {assignments.map((a) => (
          <tr key={a.variableName} style={{ borderTop: '1px solid #eaeded' }}>
            <td className="px-3 py-1.5 font-mono text-xs" style={{ color: '#16191f' }}>{a.variableName}</td>
            <td className="px-3 py-1.5 text-xs font-semibold" style={{ color: a.value ? '#1d8348' : '#d13212' }}>
              {a.value ? 'true' : 'false'}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function BooleanSolution({ data }: { data: GetBooleanSolutionResponse }) {
  if (!data.solution) return null;
  return <BoolTable assignments={data.solution.assignments} />;
}

function SatSolution({ data }: { data: GetBooleanSatisfiabilitySolutionResponse }) {
  if (!data.solution) return null;
  return <BoolTable assignments={data.solution.assignments} />;
}

function CspSolution({ data }: { data: GetCspSolutionResponse }) {
  if (!data.solution) return null;
  return (
    <div className="space-y-4">
      <div>
        <p className="text-xs font-medium mb-1" style={{ color: '#545b64' }}>Configuration</p>
        <table className="w-full text-sm border rounded overflow-hidden" style={{ borderColor: '#d5dbdb' }}>
          <thead style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
            <tr>
              <th className="text-left px-3 py-2 text-xs font-medium" style={{ color: '#545b64' }}>Variable</th>
              <th className="text-left px-3 py-2 text-xs font-medium" style={{ color: '#545b64' }}>Assigned state</th>
            </tr>
          </thead>
          <tbody>
            {data.solution.configuration.map((c) => (
              <tr key={c.name} style={{ borderTop: '1px solid #eaeded' }}>
                <td className="px-3 py-1.5 font-mono text-xs" style={{ color: '#16191f' }}>{c.name}</td>
                <td className="px-3 py-1.5 font-mono text-xs font-semibold" style={{ color: '#ff9900' }}>{c.assignedState}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {data.solution.auxiliaryAssignments.length > 0 && (
        <details>
          <summary className="text-xs cursor-pointer hover:underline" style={{ color: '#0073bb' }}>
            Auxiliary assignments ({data.solution.auxiliaryAssignments.length})
          </summary>
          <div className="mt-2">
            <BoolTable assignments={data.solution.auxiliaryAssignments} />
          </div>
        </details>
      )}
    </div>
  );
}

function VertexColoringSolution({ data }: { data: GetVertexColoringSolutionResponse }) {
  if (!data.solution) return null;
  return (
    <table className="w-full text-sm border rounded overflow-hidden" style={{ borderColor: '#d5dbdb' }}>
      <thead style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
        <tr>
          <th className="text-left px-3 py-2 text-xs font-medium" style={{ color: '#545b64' }}>Vertex</th>
          <th className="text-left px-3 py-2 text-xs font-medium" style={{ color: '#545b64' }}>Color</th>
        </tr>
      </thead>
      <tbody>
        {data.solution.colorAssignments.map((a) => (
          <tr key={a.vertexId} style={{ borderTop: '1px solid #eaeded' }}>
            <td className="px-3 py-1.5 font-mono text-xs" style={{ color: '#16191f' }}>{a.vertexId}</td>
            <td className="px-3 py-1.5 font-semibold text-xs" style={{ color: '#ff9900' }}>{a.color}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function EdgeColoringSolution({ data }: { data: GetEdgeColoringSolutionResponse }) {
  if (!data.solution) return null;
  return (
    <table className="w-full text-sm border rounded overflow-hidden" style={{ borderColor: '#d5dbdb' }}>
      <thead style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
        <tr>
          <th className="text-left px-3 py-2 text-xs font-medium" style={{ color: '#545b64' }}>Edge</th>
          <th className="text-left px-3 py-2 text-xs font-medium" style={{ color: '#545b64' }}>Color</th>
        </tr>
      </thead>
      <tbody>
        {data.solution.colorAssignments.map((a) => (
          <tr key={a.edgeLabel} style={{ borderTop: '1px solid #eaeded' }}>
            <td className="px-3 py-1.5 font-mono text-xs" style={{ color: '#16191f' }}>{a.edgeLabel}</td>
            <td className="px-3 py-1.5 font-semibold text-xs" style={{ color: '#ff9900' }}>{a.color}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

interface SolutionSectionProps {
  problemId: string;
  instanceType: string | null;
  solvingStatus: string;
}

export function SolutionSection({ problemId, instanceType, solvingStatus }: SolutionSectionProps) {
  const enabled = solvingStatus === 'Completed' && instanceType !== null;
  const { solution, loading, error } = useSolution(problemId, instanceType, enabled);

  if (!enabled) return null;

  return (
    <section className="bg-white border rounded" style={{ borderColor: '#d5dbdb' }}>
      <div className="px-4 py-3 border-b" style={{ borderColor: '#d5dbdb', background: '#fafafa' }}>
        <h2 className="text-sm font-semibold" style={{ color: '#16191f' }}>Solution</h2>
      </div>
      <div className="px-4 py-4 space-y-4">
        {loading && (
          <div className="flex items-center gap-2 text-sm" style={{ color: '#545b64' }}>
            <Spinner size="sm" /> Loading…
          </div>
        )}
        {error && <p className="text-sm" style={{ color: '#d13212' }}>{error}</p>}

        {solution && (
          <>
            <div className="flex items-center gap-3">
              <span className="text-sm" style={{ color: '#545b64' }}>Result:</span>
              <SatisfiabilityChip value={solution.satisfiability} />
            </div>

            {(() => {
              switch (instanceType) {
                case 'bool': return <BooleanSolution data={solution as GetBooleanSolutionResponse} />;
                case 'sat': return <SatSolution data={solution as GetBooleanSatisfiabilitySolutionResponse} />;
                case 'csp': return <CspSolution data={solution as GetCspSolutionResponse} />;
                case 'vertex-coloring': return <VertexColoringSolution data={solution as GetVertexColoringSolutionResponse} />;
                case 'edge-coloring': return <EdgeColoringSolution data={solution as GetEdgeColoringSolutionResponse} />;
                default: return null;
              }
            })()}
          </>
        )}
      </div>
    </section>
  );
}
