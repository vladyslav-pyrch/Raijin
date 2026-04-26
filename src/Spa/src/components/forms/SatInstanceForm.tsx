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
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Clauses (one per line, space-separated literals)
        </label>
        <textarea
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm font-mono focus:outline-none focus:ring-2 focus:ring-orange-400"
          rows={6}
          value={text}
          onChange={(e) => setText(e.target.value)}
          placeholder={'x1 ~x2 x3\n~x1 x4\nx2 ~x3 ~x4 x5'}
          disabled={loading}
        />
        {error && <p className="text-red-500 text-xs mt-1">{error}</p>}
        <p className="text-gray-400 text-xs mt-1">
          Each line is a clause. Variable name = positive literal. <code className="bg-gray-100 px-1 rounded">~name</code> = negation.
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
