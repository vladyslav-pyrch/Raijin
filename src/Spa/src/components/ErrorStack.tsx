import {useState} from 'react';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErrorEntry {
  id: string;
  message: string;
}

// ─── Component ────────────────────────────────────────────────────────────────

interface ErrorStackProps {
  errors: ErrorEntry[];
  onDismiss: (id: string) => void;
}

export function ErrorStack({ errors, onDismiss }: ErrorStackProps) {
  if (errors.length === 0) return null;

  return (
    <div className="space-y-2">
      {errors.map((e) => (
        <div
          key={e.id}
          className="flex items-start gap-3 rounded border px-4 py-3"
          style={{ background: '#fdedec', borderColor: '#d13212' }}
        >
          <span className="flex-1 text-sm" style={{ color: '#d13212' }}>
            {e.message}
          </span>
          <button
            onClick={() => onDismiss(e.id)}
            className="shrink-0 cursor-pointer text-base font-bold leading-none hover:opacity-60"
            style={{ color: '#d13212' }}
            aria-label="Dismiss"
          >
            ×
          </button>
        </div>
      ))}
    </div>
  );
}

// ─── Hook ─────────────────────────────────────────────────────────────────────

let _errId = 0;

export function useErrorStack() {
  const [errors, setErrors] = useState<ErrorEntry[]>([]);

  const addError = (message: string) => {
    const id = `err-${++_errId}`;
    setErrors((prev) => [...prev, { id, message }]);
  };

  const dismiss = (id: string) => {
    setErrors((prev) => prev.filter((e) => e.id !== id));
  };

  const clear = () => setErrors([]);

  return { errors, addError, dismiss, clear } as const;
}
