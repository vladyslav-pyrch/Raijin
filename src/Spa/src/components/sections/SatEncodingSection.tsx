import {useState} from 'react';
import {Spinner} from '../Spinner';
import type {GetSatEncodingResponse} from '../../services/combinatorics';

const PAGE_SIZE = 100;

interface SatEncodingSectionProps {
  encoding: GetSatEncodingResponse | null;
  loading: boolean;
  error: string | null;
}

export function SatEncodingSection({ encoding, loading, error }: SatEncodingSectionProps) {
  const [page, setPage] = useState(1);

  const totalPages = encoding ? Math.ceil(encoding.clauses.length / PAGE_SIZE) : 0;
  const clausePage = encoding
    ? encoding.clauses.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)
    : [];

  return (
    <section className="bg-white border rounded" style={{ borderColor: '#d5dbdb' }}>
      <div className="px-4 py-3 border-b" style={{ borderColor: '#d5dbdb', background: '#fafafa' }}>
        <h2 className="text-sm font-semibold" style={{ color: '#16191f' }}>
          Dimacs encoding
        </h2>
      </div>
      <div className="px-4 py-4">
        {loading && (
          <div className="flex items-center gap-2 text-sm" style={{ color: '#545b64' }}>
            <Spinner size="sm" /> Loading…
          </div>
        )}
        {error && <p className="text-sm" style={{ color: '#d13212' }}>{error}</p>}

        {encoding && (
          <div className="space-y-3">
            {/* Summary stats */}
            <div className="flex gap-8 text-sm">
              <span>
                <span style={{ color: '#545b64' }}>Variables: </span>
                <strong style={{ color: '#16191f' }}>{encoding.numberOfVariables}</strong>
              </span>
              <span>
                <span style={{ color: '#545b64' }}>Clauses: </span>
                <strong style={{ color: '#16191f' }}>{encoding.numberOfClauses}</strong>
              </span>
            </div>

            {encoding.clauses.length > 0 && (
              <>
                {/* Clauses table */}
                <div className="overflow-auto max-h-64 border rounded" style={{ borderColor: '#d5dbdb' }}>
                  <table className="w-full text-xs font-mono">
                    <thead
                      className="sticky top-0"
                      style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}
                    >
                      <tr>
                        <th
                          className="text-left px-3 py-2 font-medium w-16"
                          style={{ color: '#545b64' }}
                        >
                          #
                        </th>
                        <th
                          className="text-left px-3 py-2 font-medium"
                          style={{ color: '#545b64' }}
                        >
                          Literals
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {clausePage.map((clause, i) => {
                        const globalIndex = (page - 1) * PAGE_SIZE + i + 1;
                        return (
                          <tr key={globalIndex} style={{ borderTop: '1px solid #eaeded' }}>
                            <td className="px-3 py-1" style={{ color: '#879596' }}>
                              {globalIndex}
                            </td>
                            <td className="px-3 py-1" style={{ color: '#16191f' }}>
                              {clause.join(' ')}
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="flex items-center justify-between text-xs">
                    <button
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page <= 1}
                      className="rounded border px-3 py-1 cursor-pointer disabled:opacity-40"
                      style={{ borderColor: '#aab7b8', color: '#545b64', background: '#fff' }}
                    >
                      ‹ Prev
                    </button>
                    <span style={{ color: '#879596' }}>
                      Page {page} of {totalPages} &nbsp;·&nbsp;{' '}
                      {(page - 1) * PAGE_SIZE + 1}–
                      {Math.min(page * PAGE_SIZE, encoding.clauses.length)} of{' '}
                      {encoding.clauses.length} clauses
                    </span>
                    <button
                      onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                      disabled={page >= totalPages}
                      className="rounded border px-3 py-1 cursor-pointer disabled:opacity-40"
                      style={{ borderColor: '#aab7b8', color: '#545b64', background: '#fff' }}
                    >
                      Next ›
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        )}
      </div>
    </section>
  );
}
