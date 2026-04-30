import {useState} from 'react';
import {Button} from '../Button';
import {Spinner} from '../Spinner';

interface SatInstanceFormProps {
  onSubmit: (clauses: string[][]) => Promise<void>;
  loading: boolean;
  initialText?: string;
}

export function SatInstanceForm({ onSubmit, loading, initialText = '' }: SatInstanceFormProps) {
  const [text, setText] = useState(initialText);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    setError(null);
    const lines = text.trim().split('\n').filter((l) => l.trim() !== '');
    if (lines.length === 0) {
      setError('At least one clause is required');
      return;
    }

    const clauses: string[][] = [];
    for (let i = 0; i < lines.length; i++) {
      const tokens = (lines[i] ?? '').trim().split(/\s+/).filter(Boolean);
      if (tokens.length === 0) continue;
      for (const tok of tokens) {
        const name = tok.startsWith('~') ? tok.slice(1) : tok;
        if (!name) {
          setError(`Line ${i + 1}: "~" alone is not a valid literal`);
          return;
        }
      }
      clauses.push(tokens);
    }

    if (clauses.length === 0) {
      setError('At least one clause is required');
      return;
    }

    await onSubmit(clauses);
  };

  return (
    <div className="space-y-4">
      <div>
        <label className="label">
          Clauses (one per line, space-separated literals)
        </label>
        <textarea
          className="input w-full font-geist-mono resize-y"
          rows={6}
          value={text}
          onChange={(e) => setText(e.target.value)}
          placeholder={'x1 ~x2 x3\n~x1 x4\nx2 ~x3 ~x4 x5'}
          disabled={loading}
        />
        {error && <p className="text-error-500 text-xs mt-1">{error}</p>}
        <p className="text-neutral-400 dark:text-neutral-500 text-xs mt-1">
          Each line is a clause. Variable name = positive literal.{' '}
          <code className="font-geist-mono bg-neutral-100 dark:bg-surface-tertiary px-1 rounded">~name</code>{' '}
          = negation.
        </p>
      </div>
      <div className="flex justify-end">
        <Button variant="primary" onClick={handleSubmit} disabled={loading}>
          {loading && <Spinner size="sm" />}
          Set Instance
        </Button>
      </div>
    </div>
  );
}
