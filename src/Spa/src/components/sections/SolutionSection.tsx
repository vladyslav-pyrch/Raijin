import {useState} from 'react';
import {useSolution} from '../../hooks/useSolution';
import {GraphCanvas} from '../GraphCanvas';
import {Spinner} from '../Spinner';
import type {AnyInstanceData} from '../../hooks/useInstance';
import type {
    EdgeColoringInstanceDto,
    GetBooleanSatisfiabilitySolutionResponse,
    GetBooleanSolutionResponse,
    GetCspSolutionResponse,
    GetEdgeColoringSolutionResponse,
    GetVertexColoringSolutionResponse,
    VertexColoringInstanceDto,
} from '../../services/combinatorics';

// ─── Color palette ────────────────────────────────────────────────────────────

const DEFAULT_PALETTE = [
  '#e74c3c', // red
  '#3498db', // blue
  '#2ecc71', // green
  '#f39c12', // orange
  '#9b59b6', // purple
  '#1abc9c', // teal
  '#e67e22', // dark orange
  '#34495e', // dark slate
  '#e91e8c', // pink
  '#00bcd4', // cyan
];

function colorForNumber(n: number, overrides: Record<number, string>): string {
  return overrides[n] ?? DEFAULT_PALETTE[n % DEFAULT_PALETTE.length] ?? '#aab7b8';
}

// ─── Satisfiability chip ──────────────────────────────────────────────────────

function SatisfiabilityChip({ value }: { value: string | number }) {
  const str = String(value);
  const isSat = str === 'Satisfiable' || str === '1';
  const isUnsat = str === 'Unsatisfiable' || str === '2';
  const label =
    str === '1' ? 'Satisfiable'
    : str === '2' ? 'Unsatisfiable'
    : str === '0' ? 'Unknown'
    : str;
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

// ─── Color picker for a palette entry ────────────────────────────────────────

interface ColorPickerRowProps {
  colorNumber: number;
  color: string;
  onChange: (n: number, color: string) => void;
}

function ColorPickerRow({ colorNumber, color, onChange }: ColorPickerRowProps) {
  return (
    <div className="flex items-center gap-2">
      <span className="text-xs font-mono w-8 text-right" style={{ color: '#545b64' }}>
        {colorNumber}
      </span>
      <label
        className="relative cursor-pointer"
        title={`Change color for ${colorNumber}`}
      >
        <span
          className="block w-6 h-6 rounded border-2"
          style={{
            background: color,
            borderColor: color,
            boxShadow: `0 0 6px ${color}88`,
          }}
        />
        <input
          type="color"
          value={color}
          onChange={(e) => onChange(colorNumber, e.target.value)}
          className="sr-only"
        />
      </label>
      <span className="text-xs font-mono" style={{ color: '#879596' }}>{color}</span>
    </div>
  );
}

// ─── Boolean assignment table ─────────────────────────────────────────────────

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

// ─── Boolean solution ─────────────────────────────────────────────────────────

function BooleanSolution({ data }: { data: GetBooleanSolutionResponse }) {
  if (!data.solution) return null;
  return <BoolTable assignments={data.solution.assignments} />;
}

function SatSolution({ data }: { data: GetBooleanSatisfiabilitySolutionResponse }) {
  if (!data.solution) return null;
  return <BoolTable assignments={data.solution.assignments} />;
}

// ─── CSP solution ─────────────────────────────────────────────────────────────

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

// ─── Graph coloring solution (with canvas) ────────────────────────────────────

interface VertexColoringSolutionProps {
  data: GetVertexColoringSolutionResponse;
  instance: VertexColoringInstanceDto | null;
}

function VertexColoringSolution({ data, instance }: VertexColoringSolutionProps) {
  const [colorOverrides, setColorOverrides] = useState<Record<number, string>>({});

  if (!data.solution) return null;

  const { colorAssignments } = data.solution;

  // Collect distinct color numbers
  const colorNumbers = [...new Set(colorAssignments.map((a) => a.color))].sort((a, b) => a - b);

  // Build vertex → CSS color map
  const vertexColors: Record<string, string> = {};
  for (const a of colorAssignments) {
    vertexColors[a.vertexId] = colorForNumber(a.color, colorOverrides);
  }

  const handleColorChange = (n: number, c: string) =>
    setColorOverrides((prev) => ({ ...prev, [n]: c }));

  // Vertices and edges for canvas
  const vertices = instance?.graph.vertices ?? colorAssignments.map((a) => ({ id: a.vertexId }));
  const edges = instance?.graph.edges ?? [];

  return (
    <div className="space-y-3">
      <p className="text-xs" style={{ color: '#879596' }}>
        Drag vertices to rearrange. Click a color swatch to reassign.
      </p>
      <GraphCanvas
        vertices={vertices}
        edges={edges}
        vertexColors={vertexColors}
        movable
        height={320}
      />
      {/* Color legend + picker */}
      <div className="flex flex-wrap gap-4 pt-1">
        {colorNumbers.map((n) => (
          <ColorPickerRow
            key={n}
            colorNumber={n}
            color={colorForNumber(n, colorOverrides)}
            onChange={handleColorChange}
          />
        ))}
      </div>
      {/* Fallback table */}
      <details className="text-xs">
        <summary className="cursor-pointer hover:underline" style={{ color: '#0073bb' }}>
          View as table
        </summary>
        <div className="mt-2 overflow-auto max-h-48 border rounded" style={{ borderColor: '#d5dbdb' }}>
          <table className="w-full text-xs">
            <thead style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
              <tr>
                <th className="text-left px-3 py-2 font-medium" style={{ color: '#545b64' }}>Vertex</th>
                <th className="text-left px-3 py-2 font-medium" style={{ color: '#545b64' }}>Color #</th>
              </tr>
            </thead>
            <tbody>
              {colorAssignments.map((a) => (
                <tr key={a.vertexId} style={{ borderTop: '1px solid #eaeded' }}>
                  <td className="px-3 py-1 font-mono" style={{ color: '#16191f' }}>{a.vertexId}</td>
                  <td className="px-3 py-1">
                    <span
                      className="inline-flex items-center gap-1.5 font-semibold"
                      style={{ color: colorForNumber(a.color, colorOverrides) }}
                    >
                      <span
                        className="inline-block w-3 h-3 rounded-full"
                        style={{ background: colorForNumber(a.color, colorOverrides) }}
                      />
                      {a.color}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </details>
    </div>
  );
}

interface EdgeColoringSolutionProps {
  data: GetEdgeColoringSolutionResponse;
  instance: EdgeColoringInstanceDto | null;
}

function EdgeColoringSolution({ data, instance }: EdgeColoringSolutionProps) {
  const [colorOverrides, setColorOverrides] = useState<Record<number, string>>({});

  if (!data.solution) return null;

  const { colorAssignments } = data.solution;
  const colorNumbers = [...new Set(colorAssignments.map((a) => a.color))].sort((a, b) => a - b);

  // Build edge → CSS color map
  const edgeColors: Record<string, string> = {};
  for (const a of colorAssignments) {
    edgeColors[a.edgeLabel] = colorForNumber(a.color, colorOverrides);
  }

  const handleColorChange = (n: number, c: string) =>
    setColorOverrides((prev) => ({ ...prev, [n]: c }));

  const vertices = instance?.graph.vertices ?? [];
  const edges = instance?.graph.edges ?? [];

  return (
    <div className="space-y-3">
      <p className="text-xs" style={{ color: '#879596' }}>
        Drag vertices to rearrange. Click a color swatch to reassign.
      </p>
      <GraphCanvas
        vertices={vertices}
        edges={edges}
        edgeColors={edgeColors}
        movable
        height={320}
      />
      {/* Color legend + picker */}
      <div className="flex flex-wrap gap-4 pt-1">
        {colorNumbers.map((n) => (
          <ColorPickerRow
            key={n}
            colorNumber={n}
            color={colorForNumber(n, colorOverrides)}
            onChange={handleColorChange}
          />
        ))}
      </div>
      {/* Fallback table */}
      <details className="text-xs">
        <summary className="cursor-pointer hover:underline" style={{ color: '#0073bb' }}>
          View as table
        </summary>
        <div className="mt-2 overflow-auto max-h-48 border rounded" style={{ borderColor: '#d5dbdb' }}>
          <table className="w-full text-xs">
            <thead style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
              <tr>
                <th className="text-left px-3 py-2 font-medium" style={{ color: '#545b64' }}>Edge</th>
                <th className="text-left px-3 py-2 font-medium" style={{ color: '#545b64' }}>Color #</th>
              </tr>
            </thead>
            <tbody>
              {colorAssignments.map((a) => (
                <tr key={a.edgeLabel} style={{ borderTop: '1px solid #eaeded' }}>
                  <td className="px-3 py-1 font-mono" style={{ color: '#16191f' }}>{a.edgeLabel}</td>
                  <td className="px-3 py-1">
                    <span
                      className="inline-flex items-center gap-1.5 font-semibold"
                      style={{ color: colorForNumber(a.color, colorOverrides) }}
                    >
                      <span
                        className="inline-block w-3 h-3 rounded-full"
                        style={{ background: colorForNumber(a.color, colorOverrides) }}
                      />
                      {a.color}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </details>
    </div>
  );
}

// ─── Section ──────────────────────────────────────────────────────────────────

interface SolutionSectionProps {
  problemId: string;
  instanceType: string | null;
  solvingStatus: string;
  instance?: AnyInstanceData | null;
}

export function SolutionSection({
  problemId,
  instanceType,
  solvingStatus,
  instance,
}: SolutionSectionProps) {
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
                case 'bool':
                  return <BooleanSolution data={solution as GetBooleanSolutionResponse} />;
                case 'sat':
                  return <SatSolution data={solution as GetBooleanSatisfiabilitySolutionResponse} />;
                case 'csp':
                  return <CspSolution data={solution as GetCspSolutionResponse} />;
                case 'vertex-coloring':
                  return (
                    <VertexColoringSolution
                      data={solution as GetVertexColoringSolutionResponse}
                      instance={(instance as VertexColoringInstanceDto | null | undefined) ?? null}
                    />
                  );
                case 'edge-coloring':
                  return (
                    <EdgeColoringSolution
                      data={solution as GetEdgeColoringSolutionResponse}
                      instance={(instance as EdgeColoringInstanceDto | null | undefined) ?? null}
                    />
                  );
                default:
                  return null;
              }
            })()}
          </>
        )}
      </div>
    </section>
  );
}
