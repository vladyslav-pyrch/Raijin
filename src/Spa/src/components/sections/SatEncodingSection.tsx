import {Spinner} from '../Spinner';
import type {GetSatEncodingResponse} from '../../services/combinatorics';

const CLAUSE_LIMIT = 200;

interface SatEncodingSectionProps {
  encoding: GetSatEncodingResponse | null;
  loading: boolean;
  error: string | null;
}

export function SatEncodingSection({ encoding, loading, error }: SatEncodingSectionProps) {
  return (
    <section className="bg-white border rounded" style={{ borderColor: '#d5dbdb' }}>
      <div className="px-4 py-3 border-b" style={{ borderColor: '#d5dbdb', background: '#fafafa' }}>
        <h2 className="text-sm font-semibold" style={{ color: '#16191f' }}>
          SAT encoding
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
              <div className="overflow-auto max-h-64 border rounded" style={{ borderColor: '#d5dbdb' }}>
                <table className="w-full text-xs font-mono">
                  <thead className="sticky top-0" style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
                    <tr>
                      <th className="text-left px-3 py-2 font-medium w-12" style={{ color: '#545b64' }}>#</th>
                      <th className="text-left px-3 py-2 font-medium" style={{ color: '#545b64' }}>Literals</th>
                    </tr>
                  </thead>
                  <tbody>
                    {encoding.clauses.slice(0, CLAUSE_LIMIT).map((clause, i) => (
                      <tr key={i} style={{ borderTop: '1px solid #eaeded' }}>
                        <td className="px-3 py-1" style={{ color: '#879596' }}>{i + 1}</td>
                        <td className="px-3 py-1" style={{ color: '#16191f' }}>{clause.join(' ')}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {encoding.clauses.length > CLAUSE_LIMIT && (
                  <p className="text-xs px-3 py-2" style={{ borderTop: '1px solid #eaeded', color: '#879596' }}>
                    Showing first {CLAUSE_LIMIT} of {encoding.clauses.length} clauses
                  </p>
                )}
              </div>
            )}
          </div>
        )}
      </div>
    </section>
  );
}
