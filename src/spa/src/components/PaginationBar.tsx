import {useEffect, useState} from 'react';

interface PaginationBarProps {
    page: number;
    totalPages: number;
    totalItems: number;
    pageSize: number;
    onPage: (p: number) => void;
    /** Label for the items, e.g. "rows", "clauses", "variables". Default: "rows" */
    noun?: string;
}

/**
 * Pagination bar: Prev / Next buttons + range label + jump-to-page input.
 * Renders nothing when totalItems <= pageSize (no pagination needed).
 */
export function PaginationBar({
                                  page,
                                  totalPages,
                                  totalItems,
                                  pageSize,
                                  onPage,
                                  noun = 'rows',
                              }: PaginationBarProps) {
    const [jumpInput, setJumpInput] = useState(String(page));

    // Keep jump input in sync when page changes externally
    useEffect(() => {
        setJumpInput(String(page));
    }, [page]);

    if (totalItems <= pageSize) return null;

    const from = (page - 1) * pageSize + 1;
    const to = Math.min(page * pageSize, totalItems);

    const commit = () => {
        const n = parseInt(jumpInput, 10);
        if (!isNaN(n) && n >= 1 && n <= totalPages) {
            onPage(n);
        } else {
            setJumpInput(String(page)); // reset invalid input
        }
    };

    return (
        <div className="flex items-center justify-between gap-3 text-xs py-1">
            <button
                onClick={() => onPage(Math.max(1, page - 1))}
                disabled={page <= 1}
                className="btn btn-secondary btn-sm disabled:opacity-40"
            >
                ‹ Prev
            </button>

            <div className="flex items-center gap-1.5 text-neutral-400 dark:text-neutral-500 flex-wrap justify-center">
        <span>
          {from}–{to} of {totalItems} {noun}
        </span>
                <span className="opacity-50">·</span>
                <span>Page</span>
                <input
                    type="number"
                    min={1}
                    max={totalPages}
                    value={jumpInput}
                    onChange={(e) => setJumpInput(e.target.value)}
                    onBlur={commit}
                    onKeyDown={(e) => {
                        if (e.key === 'Enter') commit();
                    }}
                    className="input w-14 px-1 py-0.5 text-xs text-center"
                    aria-label="Jump to page"
                />
                <span>of {totalPages}</span>
            </div>

            <button
                onClick={() => onPage(Math.min(totalPages, page + 1))}
                disabled={page >= totalPages}
                className="btn btn-secondary btn-sm disabled:opacity-40"
            >
                Next ›
            </button>
        </div>
    );
}
