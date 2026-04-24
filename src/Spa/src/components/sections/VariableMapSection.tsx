import {useVariableMap} from '../../hooks/useVariableMap';
import {Button} from '../Button';
import {Spinner} from '../Spinner';

export function VariableMapSection({ problemId }: { problemId: string }) {
  const { variableMap, loading, error, fetched, fetch } = useVariableMap(problemId);

  return (
    <section className="bg-white border rounded" style={{ borderColor: '#d5dbdb' }}>
      <div
        className="px-4 py-3 border-b flex items-center justify-between"
        style={{ borderColor: '#d5dbdb', background: '#fafafa' }}
      >
        <h2 className="text-sm font-semibold" style={{ color: '#16191f' }}>
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
          <p className="text-sm" style={{ color: '#879596' }}>
            Click "Load variable map" to fetch the DIMACS variable index mapping.
          </p>
        </div>
      )}

      {error && (
        <div className="px-4 py-4">
          <p className="text-sm" style={{ color: '#d13212' }}>{error}</p>
        </div>
      )}

      {fetched && variableMap && (
        <div className="overflow-auto max-h-64 border-t" style={{ borderColor: '#d5dbdb' }}>
          <table className="w-full text-xs">
            <thead className="sticky top-0" style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
              <tr>
                <th className="text-left px-4 py-2 font-medium w-20" style={{ color: '#545b64' }}>Index</th>
                <th className="text-left px-4 py-2 font-medium" style={{ color: '#545b64' }}>Variable name</th>
              </tr>
            </thead>
            <tbody>
              {variableMap.variables.map((v) => (
                <tr key={v.index} style={{ borderTop: '1px solid #eaeded' }}>
                  <td className="px-4 py-1.5 font-mono" style={{ color: '#545b64' }}>{v.index}</td>
                  <td className="px-4 py-1.5 font-mono" style={{ color: '#16191f' }}>{v.name}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
