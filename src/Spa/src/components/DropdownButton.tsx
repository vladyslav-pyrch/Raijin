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
          'inline-flex rounded border overflow-hidden',
          isDisabled ? 'opacity-50' : '',
        ]
          .filter(Boolean)
          .join(' ')}
        style={{ borderColor: '#e88b00' }}
      >
        <button
          disabled={isDisabled}
          onClick={() => setOpen((o) => !o)}
          className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-semibold bg-[#ff9900] hover:bg-[#e88b00] text-[#16191f] border-r cursor-pointer transition-colors disabled:cursor-not-allowed"
          style={{ borderColor: '#e88b00' }}
        >
          {loading && <Spinner size="sm" />}
          {label}
        </button>
        <button
          disabled={isDisabled}
          onClick={() => setOpen((o) => !o)}
          className="flex items-center px-2 py-1.5 bg-[#ff9900] hover:bg-[#e88b00] text-[#16191f] cursor-pointer transition-colors disabled:cursor-not-allowed"
          aria-label="Show options"
        >
          <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M19 9l-7 7-7-7" />
          </svg>
        </button>
      </div>

      {open && (
        <div className="absolute top-full left-0 mt-0.5 w-44 bg-white border border-[#d5dbdb] rounded shadow-lg z-20 overflow-hidden">
          {options.map((opt) => (
            <button
              key={opt.value}
              onClick={() => {
                setOpen(false);
                onSelect(opt.value);
              }}
              className="w-full text-left px-3 py-2 text-sm text-[#16191f] hover:bg-[#f2f3f3] cursor-pointer transition-colors"
            >
              {opt.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
