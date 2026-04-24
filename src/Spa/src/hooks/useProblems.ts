import {useCallback, useEffect, useState} from 'react';
import {api} from '../lib/api';
import type {ProblemSummaryResponse} from '../services/combinatorics';

interface UseProblemsResult {
  problems: ProblemSummaryResponse[];
  loading: boolean;
  error: string | null;
  totalPages: number;
  page: number;
  loadPage: (page: number) => void;
  refresh: () => void;
}

export function useProblems(): UseProblemsResult {
  const [problems, setProblems] = useState<ProblemSummaryResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalPages, setTotalPages] = useState(1);
  const [page, setPage] = useState(1);
  const [tick, setTick] = useState(0);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    api
      .listProblems(page)
      .then((res) => {
        if (cancelled) return;
        setProblems(res.items);
        setTotalPages(res.totalPages);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : 'Failed to load problems');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [page, tick]);

  const loadPage = useCallback((p: number) => setPage(p), []);
  const refresh = useCallback(() => setTick((t) => t + 1), []);

  return { problems, loading, error, totalPages, page, loadPage, refresh };
}
