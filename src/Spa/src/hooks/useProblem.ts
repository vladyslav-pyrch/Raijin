import {useCallback, useEffect, useState} from 'react';
import {api} from '../lib/api';
import type {GetProblemResponse} from '../services/combinatorics';

interface UseProblemResult {
  problem: GetProblemResponse | null;
  loading: boolean;
  error: string | null;
  refresh: () => void;
}

export function useProblem(id: string): UseProblemResult {
  const [problem, setProblem] = useState<GetProblemResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tick, setTick] = useState(0);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    api
      .getProblem(id)
      .then((res) => {
        if (!cancelled) setProblem(res);
      })
      .catch((err: unknown) => {
        if (!cancelled)
          setError(err instanceof Error ? err.message : 'Failed to load problem');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [id, tick]);

  const refresh = useCallback(() => setTick((t) => t + 1), []);

  return { problem, loading, error, refresh };
}
