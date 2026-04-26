import {useState} from 'react';
import {Button} from '../Button';
import {Spinner} from '../Spinner';
import type {CspInstanceDto} from '../../services/combinatorics';

interface Variable {
  name: string;
  states: string;
}

interface CspInstanceFormProps {
  onSubmit: (instance: CspInstanceDto) => Promise<void>;
  loading: boolean;
  initialVariables?: Variable[];
  initialConstraints?: string[];
}

export function CspInstanceForm({ onSubmit, loading, initialVariables, initialConstraints }: CspInstanceFormProps) {
  const [variables, setVariables] = useState<Variable[]>(initialVariables ?? [{ name: '', states: '' }]);
  const [constraints, setConstraints] = useState<string[]>(initialConstraints ?? ['']);
  const [error, setError] = useState<string | null>(null);

  const updateVariable = (index: number, field: keyof Variable, value: string) => {
    setVariables((prev) =>
      prev.map((v, i) => (i === index ? { ...v, [field]: value } : v)),
    );
  };

  const removeVariable = (index: number) => {
    setVariables((prev) => prev.filter((_, i) => i !== index));
  };

  const updateConstraint = (index: number, value: string) => {
    setConstraints((prev) => prev.map((c, i) => (i === index ? value : c)));
  };

  const removeConstraint = (index: number) => {
    setConstraints((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async () => {
    setError(null);
    for (let i = 0; i < variables.length; i++) {
      const v = variables[i];
      if (!v || !v.name.trim()) {
        setError(`Variable ${i + 1}: name is required`);
        return;
      }
      const states = v.states
        .split(',')
        .map((s) => s.trim())
        .filter(Boolean);
      if (states.length < 2) {
        setError(`Variable "${v.name}": at least 2 states required (comma-separated)`);
        return;
      }
    }

    const instance: CspInstanceDto = {
      variables: variables.map((v) => ({
        name: v.name.trim(),
        states: v.states
          .split(',')
          .map((s) => s.trim())
          .filter(Boolean),
      })),
      constraints: constraints.filter((c) => c.trim() !== ''),
    };

    await onSubmit(instance);
  };

  return (
    <div className="space-y-5 max-h-[60vh] overflow-y-auto pr-1">
      <div>
        <div className="flex items-center justify-between mb-2">
          <label className="text-sm font-medium text-gray-700">Variables</label>
          <Button
            size="sm"
            variant="ghost"
            onClick={() => setVariables((v) => [...v, { name: '', states: '' }])}
            disabled={loading}
          >
            + Add Variable
          </Button>
        </div>
        <div className="space-y-2">
          {variables.map((v, i) => (
            <div key={i} className="flex gap-2 items-start">
              <input
                className="flex-1 border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-orange-400"
                placeholder="Name"
                value={v.name}
                onChange={(e) => updateVariable(i, 'name', e.target.value)}
                disabled={loading}
              />
              <input
                className="flex-2 border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-orange-400"
                placeholder="States (comma-separated)"
                value={v.states}
                onChange={(e) => updateVariable(i, 'states', e.target.value)}
                disabled={loading}
              />
              <Button
                size="sm"
                variant="ghost"
                onClick={() => removeVariable(i)}
                disabled={loading || variables.length <= 1}
              >
                ×
              </Button>
            </div>
          ))}
        </div>
      </div>

      <div>
        <div className="flex items-center justify-between mb-2">
          <label className="text-sm font-medium text-gray-700">
            Constraints (optional)
          </label>
          <Button
            size="sm"
            variant="ghost"
            onClick={() => setConstraints((c) => [...c, ''])}
            disabled={loading}
          >
            + Add Constraint
          </Button>
        </div>
        <div className="space-y-2">
          {constraints.map((c, i) => (
            <div key={i} className="flex gap-2">
              <input
                className="flex-1 border border-gray-300 rounded-lg px-3 py-1.5 text-sm font-mono focus:outline-none focus:ring-2 focus:ring-orange-400"
                placeholder="e.g. colour::red | colour::blue & !colour::green"
                value={c}
                onChange={(e) => updateConstraint(i, e.target.value)}
                disabled={loading}
              />
              <Button
                size="sm"
                variant="ghost"
                onClick={() => removeConstraint(i)}
                disabled={loading}
              >
                ×
              </Button>
            </div>
          ))}
        </div>
      </div>

      {error && <p className="text-red-500 text-sm">{error}</p>}

      <div className="flex justify-end">
        <Button variant="primary" onClick={handleSubmit} disabled={loading}>
          {loading && <Spinner size="sm" />}
          Set Instance
        </Button>
      </div>
    </div>
  );
}
