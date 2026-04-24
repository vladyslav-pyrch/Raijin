import {useCallback, useState} from 'react';
import {api} from '../lib/api';
import type {GetSatEncodingVariableMapResponse} from '../services/combinatorics';

interface UseVariableMapResult {
  variableMap: GetSatEncodingVariableMapResponse | null;
  loading: boolean;
  error: string | null;
  fetched: boolean;
  fetch: () => void;
}

export function useVariableMap(id: string): UseVariableMapResult {
  const [variableMap, setVariableMap] =
    useState<GetSatEncodingVariableMapResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fetched, setFetched] = useState(false);

  const fetch = useCallback(() => {
    setLoading(true);
    setError(null);
    api
      .getSatEncodingVariableMap(id)
      .then((res) => {
        setVariableMap(res);
        setFetched(true);
      })
      .catch((err: unknown) => {
        setError(err instanceof Error ? err.message : 'Failed to load variable map');
      })
      .finally(() => {
        setLoading(false);
      });
  }, [id]);

  return { variableMap, loading, error, fetched, fetch };
}
