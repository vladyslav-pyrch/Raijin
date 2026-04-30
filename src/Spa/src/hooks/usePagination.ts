import {useEffect, useState} from 'react';

interface UsePaginationResult<T> {
  page: number;
  totalPages: number;
  pageItems: T[];
  setPage: (p: number) => void;
}

/**
 * Slices `items` into pages of `pageSize`.
 * Resets to page 1 whenever the total item count changes (e.g. new data loaded).
 */
export function usePagination<T>(items: T[], pageSize: number): UsePaginationResult<T> {
  const [page, setPageRaw] = useState(1);

  const totalPages = Math.max(1, Math.ceil(items.length / pageSize));
  const safePage = Math.min(page, totalPages);

  // Reset to page 1 when dataset changes
  useEffect(() => {
    setPageRaw(1);
  }, [items.length]);

  const setPage = (p: number) => setPageRaw(Math.min(totalPages, Math.max(1, p)));

  const pageItems = items.slice((safePage - 1) * pageSize, safePage * pageSize);

  return { page: safePage, totalPages, pageItems, setPage };
}
