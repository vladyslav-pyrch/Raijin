import {useVariableMap} from '../../hooks/useVariableMap';
import {usePagination} from '../../hooks/usePagination';
import {Button} from '../Button';
import {Spinner} from '../Spinner';
import {PaginationBar} from '../PaginationBar';

const PAGE_SIZE = 100;

export function VariableMapSection({ problemId }: { problemId: string }) {
  const { variableMap, loading, error, fetched, fetch } = useVariableMap(problemId);

  const variables = variableMap?.variables ?? [];
  const { page, totalPages, pageItems, setPage } = usePagination(variables, PAGE_SIZE);

  return (
    <section className="card">
      <div className="card-header flex items-center justify-between">
        <h2 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">
          Variable map
        </h2>
        {!fetched && (
          <Button size="sm" variant="secondary" onClick={fetch} disabled={loading}>
            {loading && <Spinner size="sm" />}
            Load variable map
          </Button>
        )}
      </div>

      {!fetched && !loading && !error && (
        <div className="px-4 py-4">
          <p className="text-sm text-neutral-400 dark:text-neutral-500">
            Click "Load variable map" to fetch the DIMACS variable index mapping.
          </p>
        </div>
      )}

      {error && (
        <div className="px-4 py-4">
          <p className="text-sm text-error-500">{error}</p>
        </div>
      )}

      {fetched && variableMap && (
        <div className="px-4 py-3 space-y-2">
          {/* Summary */}
          <p className="text-xs text-neutral-500 dark:text-neutral-400">
            {variables.length} variable{variables.length !== 1 ? 's' : ''}
          </p>

          {/* Table */}
          <div className="overflow-auto max-h-64 border border-neutral-200 dark:border-neutral-700 rounded-md">
            <table className="w-full text-xs">
              <thead className="table-header sticky top-0">
                <tr>
                  <th className="text-left px-4 py-2 font-medium w-20 text-neutral-500 dark:text-neutral-400">Index</th>
                  <th className="text-left px-4 py-2 font-medium text-neutral-500 dark:text-neutral-400">Variable name</th>
                </tr>
              </thead>
              <tbody>
                {pageItems.map((v) => (
                  <tr key={v.index} className="table-row">
                    <td className="px-4 py-1.5 font-geist-mono text-neutral-500 dark:text-neutral-400">{v.index}</td>
                    <td className="px-4 py-1.5 font-geist-mono text-neutral-900 dark:text-neutral-100">{v.name}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <PaginationBar
            page={page}
            totalPages={totalPages}
            totalItems={variables.length}
            pageSize={PAGE_SIZE}
            onPage={setPage}
            noun="variables"
          />
        </div>
      )}
    </section>
  );
}
