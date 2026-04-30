import {useState} from 'react';
import {Spinner} from '../Spinner';
import {PaginationBar} from '../PaginationBar';
import type {GetSatEncodingResponse} from '../../services/combinatorics';

const PAGE_SIZE = 100;

interface SatEncodingSectionProps {
  encoding: GetSatEncodingResponse | null;
  loading: boolean;
  error: string | null;
}

export function SatEncodingSection({ encoding, loading, error }: SatEncodingSectionProps) {
  const [page, setPage] = useState(1);

  const clauses = encoding?.clauses ?? [];
  const totalPages = Math.max(1, Math.ceil(clauses.length / PAGE_SIZE));
  const safePage = Math.min(page, totalPages);
  const clausePage = clauses.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE);

  return (
    <section className="card">
      <div className="card-header">
        <h2 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">
          Dimacs encoding
        </h2>
      </div>
      <div className="px-4 py-4">
        {loading && (
          <div className="flex items-center gap-2 text-sm text-neutral-500 dark:text-neutral-400">
            <Spinner size="sm" /> Loading…
          </div>
        )}
        {error && <p className="text-sm text-error-500">{error}</p>}

        {encoding && (
          <div className="space-y-3">
            {/* Summary stats */}
            <div className="flex gap-8 text-sm">
              <span>
                <span className="text-neutral-500 dark:text-neutral-400">Variables: </span>
                <strong className="text-neutral-900 dark:text-neutral-100">{encoding.numberOfVariables}</strong>
              </span>
              <span>
                <span className="text-neutral-500 dark:text-neutral-400">Clauses: </span>
                <strong className="text-neutral-900 dark:text-neutral-100">{encoding.numberOfClauses}</strong>
              </span>
            </div>

            {clauses.length > 0 && (
              <>
                <div className="overflow-auto max-h-64 border border-neutral-200 dark:border-neutral-700 rounded-md">
                  <table className="w-full text-xs font-geist-mono">
                    <thead className="table-header sticky top-0">
                      <tr>
                        <th className="text-left px-3 py-2 font-medium w-16 text-neutral-500 dark:text-neutral-400">#</th>
                        <th className="text-left px-3 py-2 font-medium text-neutral-500 dark:text-neutral-400">Literals</th>
                      </tr>
                    </thead>
                    <tbody>
                      {clausePage.map((clause, i) => {
                        const globalIndex = (safePage - 1) * PAGE_SIZE + i + 1;
                        return (
                          <tr key={globalIndex} className="table-row">
                            <td className="px-3 py-1 text-neutral-400 dark:text-neutral-500">
                              {globalIndex}
                            </td>
                            <td className="px-3 py-1 text-neutral-900 dark:text-neutral-100">
                              {clause.join(' ')}
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>

                <PaginationBar
                  page={safePage}
                  totalPages={totalPages}
                  totalItems={clauses.length}
                  pageSize={PAGE_SIZE}
                  onPage={setPage}
                  noun="clauses"
                />
              </>
            )}
          </div>
        )}
      </div>
    </section>
  );
}
