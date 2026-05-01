import {Spinner} from '../Spinner';
import {GraphCanvas} from '../GraphCanvas';
import {PaginationBar} from '../PaginationBar';
import {usePagination} from '../../hooks/usePagination';
import type {AnyInstanceData} from '../../hooks/useInstance';
import type {
    BooleanProblemInstanceDto,
    CspInstanceDto,
    EdgeColoringInstanceDto,
    SatInstanceDto,
    VertexColoringInstanceDto,
} from '../../services/combinatorics';

// ─── Per-type renderers ───────────────────────────────────────────────────────

function BooleanInstance({data}: { data: BooleanProblemInstanceDto }) {
    return (
        <div>
            <p className="text-xs font-medium mb-1 text-neutral-500 dark:text-neutral-400">Formula</p>
            <pre className="code-block whitespace-pre-wrap break-all">
        {data.formula}
      </pre>
        </div>
    );
}

const SAT_CLAUSE_PAGE_SIZE = 100;

function SatInstance({data}: { data: SatInstanceDto }) {
    const {page, totalPages, pageItems, setPage} = usePagination(data.clauses, SAT_CLAUSE_PAGE_SIZE);
    const from = (page - 1) * SAT_CLAUSE_PAGE_SIZE + 1;

    return (
        <div className="space-y-2">
            <p className="text-xs font-medium text-neutral-500 dark:text-neutral-400">
                Clauses ({data.clauses.length})
            </p>
            <div className="overflow-auto max-h-56 border border-neutral-200 dark:border-neutral-700 rounded-md">
                <table className="w-full text-xs font-geist-mono">
                    <thead className="table-header sticky top-0">
                    <tr>
                        <th className="text-left px-3 py-2 font-medium w-12 text-neutral-500 dark:text-neutral-400">#</th>
                        <th className="text-left px-3 py-2 font-medium text-neutral-500 dark:text-neutral-400">Literals</th>
                    </tr>
                    </thead>
                    <tbody>
                    {pageItems.map((clause: string[], i: number) => (
                        <tr key={from + i} className="table-row">
                            <td className="px-3 py-1 text-neutral-400 dark:text-neutral-500">{from + i}</td>
                            <td className="px-3 py-1 text-neutral-900 dark:text-neutral-100">{clause.join('  ')}</td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>
            <PaginationBar
                page={page}
                totalPages={totalPages}
                totalItems={data.clauses.length}
                pageSize={SAT_CLAUSE_PAGE_SIZE}
                onPage={setPage}
                noun="clauses"
            />
        </div>
    );
}

function CspInstance({data}: { data: CspInstanceDto }) {
    return (
        <div className="space-y-4">
            <div>
                <p className="text-xs font-medium mb-1 text-neutral-500 dark:text-neutral-400">
                    Variables ({data.variables.length})
                </p>
                <div className="overflow-auto max-h-48 border border-neutral-200 dark:border-neutral-700 rounded-md">
                    <table className="w-full text-sm">
                        <thead className="table-header sticky top-0">
                        <tr>
                            <th className="text-left px-3 py-2 text-xs font-medium text-neutral-500 dark:text-neutral-400">Name</th>
                            <th className="text-left px-3 py-2 text-xs font-medium text-neutral-500 dark:text-neutral-400">States</th>
                        </tr>
                        </thead>
                        <tbody>
                        {data.variables.map((v) => (
                            <tr key={v.name} className="table-row">
                                <td className="px-3 py-1.5 font-geist-mono text-xs text-neutral-900 dark:text-neutral-100">{v.name}</td>
                                <td className="px-3 py-1.5 text-xs">
                                    {v.states.map((s, i) => (
                                        <span
                                            key={i}
                                            className="inline-block mr-1 mb-0.5 px-1.5 py-0.5 rounded text-xs
                                   bg-primary-50 dark:bg-primary-900/30
                                   text-primary-700 dark:text-primary-300
                                   border border-primary-200 dark:border-primary-800"
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
                    <p className="text-xs font-medium mb-1 text-neutral-500 dark:text-neutral-400">
                        Constraints ({data.constraints.length})
                    </p>
                    <ul className="space-y-1">
                        {data.constraints.map((c, i) => (
                            <li
                                key={i}
                                className="text-xs font-geist-mono rounded-md px-3 py-1.5
                           bg-neutral-100 dark:bg-surface-tertiary
                           border border-neutral-200 dark:border-neutral-700
                           text-neutral-900 dark:text-neutral-100"
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
    const {graph, colorCount} = data;

    return (
        <div className="space-y-3">
            <div className="flex gap-8 text-sm">
        <span>
          <span className="text-neutral-500 dark:text-neutral-400">Vertices: </span>
          <strong className="text-neutral-900 dark:text-neutral-100">{graph.vertices.length}</strong>
        </span>
                <span>
          <span className="text-neutral-500 dark:text-neutral-400">Edges: </span>
          <strong className="text-neutral-900 dark:text-neutral-100">{graph.edges.length}</strong>
        </span>
                <span>
          <span className="text-neutral-500 dark:text-neutral-400">Colors ({label}): </span>
          <strong className="text-neutral-900 dark:text-neutral-100">{colorCount}</strong>
        </span>
            </div>
            <p className="text-xs text-neutral-400 dark:text-neutral-500">
                Drag vertices to rearrange layout. Scroll to zoom.
            </p>
            <GraphCanvas vertices={graph.vertices} edges={graph.edges} movable height={300}/>
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

export function InstanceSection({instanceType, instance, loading, error}: InstanceSectionProps) {
    if (!instanceType) return null;

    return (
        <section className="card">
            <div className="card-header flex items-center gap-3">
                <h2 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">
                    Problem instance
                </h2>
                <span className="badge badge-warning">{instanceType}</span>
            </div>

            <div className="px-4 py-4">
                {loading && (
                    <div className="flex items-center gap-2 text-sm text-neutral-500 dark:text-neutral-400">
                        <Spinner size="sm"/> Loading instance…
                    </div>
                )}
                {error && <p className="text-sm text-error-500">{error}</p>}

                {instance && (
                    <>
                        {instanceType === 'bool' && <BooleanInstance data={instance as BooleanProblemInstanceDto}/>}
                        {instanceType === 'sat' && <SatInstance data={instance as SatInstanceDto}/>}
                        {instanceType === 'csp' && <CspInstance data={instance as CspInstanceDto}/>}
                        {instanceType === 'vertex-coloring' && (
                            <GraphInstance data={instance as VertexColoringInstanceDto} label="vertex"/>
                        )}
                        {instanceType === 'edge-coloring' && (
                            <GraphInstance data={instance as EdgeColoringInstanceDto} label="edge"/>
                        )}
                    </>
                )}
            </div>
        </section>
    );
}
