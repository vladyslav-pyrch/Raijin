import {useState} from 'react';
import {useSolution} from '../../hooks/useSolution';
import {usePagination} from '../../hooks/usePagination';
import {GraphCanvas} from '../GraphCanvas';
import {Spinner} from '../Spinner';
import {PaginationBar} from '../PaginationBar';
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

// ─── Color palette (data-level, not UI styling — intentionally stays as hex) ──

const DEFAULT_PALETTE = [
  '#e74c3c', '#3498db', '#2ecc71', '#f39c12', '#9b59b6',
  '#1abc9c', '#e67e22', '#34495e', '#e91e8c', '#00bcd4',
];

function colorForNumber(n: number, overrides: Record<number, string>): string {
  return overrides[n] ?? DEFAULT_PALETTE[n % DEFAULT_PALETTE.length] ?? '#9CA3AF';
}

// ─── Satisfiability chip ──────────────────────────────────────────────────────

function SatisfiabilityChip({ value }: { value: string | number }) {
  const str = String(value);
  const isSat   = str === 'Satisfiable'   || str === '1';
  const isUnsat = str === 'Unsatisfiable' || str === '2';
  const label =
    str === '1' ? 'Satisfiable'
    : str === '2' ? 'Unsatisfiable'
    : str === '0' ? 'Unknown'
    : str;

  const cls = isSat
    ? 'badge badge-success'
    : isUnsat
      ? 'badge badge-error'
      : 'badge';

  return <span className={cls}>{label}</span>;
}

// ─── Color picker row ─────────────────────────────────────────────────────────

interface ColorPickerRowProps {
  colorNumber: number;
  color: string;
  onChange: (n: number, color: string) => void;
}

function ColorPickerRow({ colorNumber, color, onChange }: ColorPickerRowProps) {
  return (
    <div className="flex items-center gap-2">
      <span className="text-xs font-geist-mono w-8 text-right text-neutral-500 dark:text-neutral-400">
        {colorNumber}
      </span>
      <label className="relative cursor-pointer" title={`Change color for ${colorNumber}`}>
        <span
          className="block w-6 h-6 rounded border-2"
          style={{ background: color, borderColor: color, boxShadow: `0 0 6px ${color}88` }}
        />
        <input
          type="color"
          value={color}
          onChange={(e) => onChange(colorNumber, e.target.value)}
          className="sr-only"
        />
      </label>
      <span className="text-xs font-geist-mono text-neutral-400 dark:text-neutral-500">{color}</span>
    </div>
  );
}

// ─── Boolean assignment table ─────────────────────────────────────────────────

function BoolTable({ assignments }: { assignments: { variableName: string; value: boolean }[] }) {
  return (
    <table className="w-full text-sm border border-neutral-200 dark:border-neutral-700 rounded-md overflow-hidden">
      <thead className="table-header">
        <tr>
          <th className="text-left px-3 py-2 text-xs font-medium text-neutral-500 dark:text-neutral-400">Variable</th>
          <th className="text-left px-3 py-2 text-xs font-medium text-neutral-500 dark:text-neutral-400">Value</th>
        </tr>
      </thead>
      <tbody>
        {assignments.map((a) => (
          <tr key={a.variableName} className="table-row">
            <td className="px-3 py-1.5 font-geist-mono text-xs text-neutral-900 dark:text-neutral-100">{a.variableName}</td>
            <td className={`px-3 py-1.5 text-xs font-semibold ${a.value ? 'text-success-500' : 'text-error-500'}`}>
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
        <p className="text-xs font-medium mb-1 text-neutral-500 dark:text-neutral-400">Configuration</p>
        <table className="w-full text-sm border border-neutral-200 dark:border-neutral-700 rounded-md overflow-hidden">
          <thead className="table-header">
            <tr>
              <th className="text-left px-3 py-2 text-xs font-medium text-neutral-500 dark:text-neutral-400">Variable</th>
              <th className="text-left px-3 py-2 text-xs font-medium text-neutral-500 dark:text-neutral-400">Assigned state</th>
            </tr>
          </thead>
          <tbody>
            {data.solution.configuration.map((c) => (
              <tr key={c.name} className="table-row">
                <td className="px-3 py-1.5 font-geist-mono text-xs text-neutral-900 dark:text-neutral-100">{c.name}</td>
                <td className="px-3 py-1.5 font-geist-mono text-xs font-semibold text-primary-500 dark:text-primary-400">{c.assignedState}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {data.solution.auxiliaryAssignments.length > 0 && (
        <details>
          <summary className="text-xs cursor-pointer link">
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

// ─── Graph coloring solutions ─────────────────────────────────────────────────

interface VertexColoringSolutionProps {
  data: GetVertexColoringSolutionResponse;
  instance: VertexColoringInstanceDto | null;
}

const COLORING_PAGE_SIZE = 50;

function VertexColoringSolution({ data, instance }: VertexColoringSolutionProps) {
  const [colorOverrides, setColorOverrides] = useState<Record<number, string>>({});
  if (!data.solution) return null;

  const { colorAssignments } = data.solution;
  const colorNumbers = [...new Set(colorAssignments.map((a) => a.color))].sort((a, b) => a - b);

  const vertexColors: Record<string, string> = {};
  for (const a of colorAssignments) {
    vertexColors[a.vertexId] = colorForNumber(a.color, colorOverrides);
  }

  const handleColorChange = (n: number, c: string) =>
    setColorOverrides((prev) => ({ ...prev, [n]: c }));

  const vertices = instance?.graph.vertices ?? colorAssignments.map((a) => ({ id: a.vertexId }));
  const edges = instance?.graph.edges ?? [];

  return (
    <div className="space-y-3">
      <p className="text-xs text-neutral-400 dark:text-neutral-500">
        Drag vertices to rearrange. Click a color swatch to reassign.
      </p>
      <GraphCanvas vertices={vertices} edges={edges} vertexColors={vertexColors} movable height={320} />
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
      <details className="text-xs">
        <summary className="cursor-pointer link">View as table</summary>
        <VertexColoringTable
          colorAssignments={colorAssignments}
          colorOverrides={colorOverrides}
        />
      </details>
    </div>
  );
}

function VertexColoringTable({
  colorAssignments,
  colorOverrides,
}: {
  colorAssignments: { vertexId: string; color: number }[];
  colorOverrides: Record<number, string>;
}) {
  const { page, totalPages, pageItems, setPage } = usePagination(colorAssignments, COLORING_PAGE_SIZE);
  return (
    <div className="mt-2 space-y-2">
      <div className="overflow-auto max-h-48 border border-neutral-200 dark:border-neutral-700 rounded-md">
        <table className="w-full text-xs">
          <thead className="table-header sticky top-0">
            <tr>
              <th className="text-left px-3 py-2 font-medium text-neutral-500 dark:text-neutral-400">Vertex</th>
              <th className="text-left px-3 py-2 font-medium text-neutral-500 dark:text-neutral-400">Color #</th>
            </tr>
          </thead>
          <tbody>
            {pageItems.map((a) => (
              <tr key={a.vertexId} className="table-row">
                <td className="px-3 py-1 font-geist-mono text-neutral-900 dark:text-neutral-100">{a.vertexId}</td>
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
      <PaginationBar
        page={page}
        totalPages={totalPages}
        totalItems={colorAssignments.length}
        pageSize={COLORING_PAGE_SIZE}
        onPage={setPage}
        noun="vertices"
      />
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
      <p className="text-xs text-neutral-400 dark:text-neutral-500">
        Drag vertices to rearrange. Click a color swatch to reassign.
      </p>
      <GraphCanvas vertices={vertices} edges={edges} edgeColors={edgeColors} movable height={320} />
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
      <details className="text-xs">
        <summary className="cursor-pointer link">View as table</summary>
        <EdgeColoringTable
          colorAssignments={colorAssignments}
          colorOverrides={colorOverrides}
        />
      </details>
    </div>
  );
}

function EdgeColoringTable({
  colorAssignments,
  colorOverrides,
}: {
  colorAssignments: { edgeLabel: string; color: number }[];
  colorOverrides: Record<number, string>;
}) {
  const { page, totalPages, pageItems, setPage } = usePagination(colorAssignments, COLORING_PAGE_SIZE);
  return (
    <div className="mt-2 space-y-2">
      <div className="overflow-auto max-h-48 border border-neutral-200 dark:border-neutral-700 rounded-md">
        <table className="w-full text-xs">
          <thead className="table-header sticky top-0">
            <tr>
              <th className="text-left px-3 py-2 font-medium text-neutral-500 dark:text-neutral-400">Edge</th>
              <th className="text-left px-3 py-2 font-medium text-neutral-500 dark:text-neutral-400">Color #</th>
            </tr>
          </thead>
          <tbody>
            {pageItems.map((a) => (
              <tr key={a.edgeLabel} className="table-row">
                <td className="px-3 py-1 font-geist-mono text-neutral-900 dark:text-neutral-100">{a.edgeLabel}</td>
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
      <PaginationBar
        page={page}
        totalPages={totalPages}
        totalItems={colorAssignments.length}
        pageSize={COLORING_PAGE_SIZE}
        onPage={setPage}
        noun="edges"
      />
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
    <section className="card">
      <div className="card-header">
        <h2 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">Solution</h2>
      </div>
      <div className="px-4 py-4 space-y-4">
        {loading && (
          <div className="flex items-center gap-2 text-sm text-neutral-500 dark:text-neutral-400">
            <Spinner size="sm" /> Loading…
          </div>
        )}
        {error && <p className="text-sm text-error-500">{error}</p>}

        {solution && (
          <>
            <div className="flex items-center gap-3">
              <span className="text-sm text-neutral-500 dark:text-neutral-400">Result:</span>
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
