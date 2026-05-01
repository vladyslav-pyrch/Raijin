import {useEffect, useRef, useState} from 'react';
import {Spinner} from './Spinner';

interface Option {
    label: string;
    value: string;
}

interface DropdownButtonProps {
    label: string;
    options: Option[];
    onSelect: (value: string) => void;
    loading?: boolean;
    disabled?: boolean;
}

export function DropdownButton({
                                   label,
                                   options,
                                   onSelect,
                                   loading = false,
                                   disabled = false,
                               }: DropdownButtonProps) {
    const [open, setOpen] = useState(false);
    const ref = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!open) return;
        const handler = (e: MouseEvent) => {
            if (ref.current && !ref.current.contains(e.target as Node)) {
                setOpen(false);
            }
        };
        document.addEventListener('mousedown', handler);
        return () => document.removeEventListener('mousedown', handler);
    }, [open]);

    const isDisabled = disabled || loading;

    return (
        <div className="relative inline-flex" ref={ref}>
            <div
                className={[
                    'inline-flex rounded-md border overflow-hidden border-primary-600',
                    isDisabled ? 'opacity-50' : '',
                ]
                    .filter(Boolean)
                    .join(' ')}
            >
                {/* Main label button */}
                <button
                    disabled={isDisabled}
                    onClick={() => setOpen((o) => !o)}
                    className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-semibold
                     bg-primary-500 hover:bg-primary-600 text-white
                     border-r border-primary-600
                     cursor-pointer transition-colors disabled:cursor-not-allowed"
                >
                    {loading && <Spinner size="sm"/>}
                    {label}
                </button>

                {/* Chevron button */}
                <button
                    disabled={isDisabled}
                    onClick={() => setOpen((o) => !o)}
                    className="flex items-center px-2 py-1.5
                     bg-primary-500 hover:bg-primary-600 text-white
                     cursor-pointer transition-colors disabled:cursor-not-allowed"
                    aria-label="Show options"
                >
                    <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M19 9l-7 7-7-7"/>
                    </svg>
                </button>
            </div>

            {open && (
                <div className="absolute top-full left-0 mt-0.5 w-48
                        bg-white dark:bg-surface-secondary
                        border border-neutral-200 dark:border-neutral-700
                        rounded-md shadow-lg z-20 overflow-hidden animate-fade-in">
                    {options.map((opt) => (
                        <button
                            key={opt.value}
                            onClick={() => {
                                setOpen(false);
                                onSelect(opt.value);
                            }}
                            className="w-full text-left px-3 py-2 text-sm
                         text-neutral-900 dark:text-neutral-100
                         hover:bg-neutral-100 dark:hover:bg-surface-tertiary
                         cursor-pointer transition-colors"
                        >
                            {opt.label}
                        </button>
                    ))}
                </div>
            )}
        </div>
    );
}
