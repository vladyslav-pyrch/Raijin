import {useEffect, useState} from 'react';
import {api} from '../lib/api';
import type {GetSatEncodingResponse} from '../services/combinatorics';

interface UseSatEncodingResult {
  encoding: GetSatEncodingResponse | null;
  loading: boolean;
  error: string | null;
}

export function useSatEncoding(id: string, enabled: boolean): UseSatEncodingResult {
  const [encoding, setEncoding] = useState<GetSatEncodingResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!enabled) return;
    let cancelled = false;
    setLoading(true);
    setError(null);
    api
      .getSatEncoding(id)
      .then((res) => {
        if (!cancelled) setEncoding(res);
      })
      .catch((err: unknown) => {
        if (!cancelled)
          setError(err instanceof Error ? err.message : 'Failed to load SAT encoding');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [id, enabled]);

  return { encoding, loading, error };
}
