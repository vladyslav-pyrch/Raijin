import {useState} from 'react';
import {Button} from '../Button';
import {Spinner} from '../Spinner';

interface BooleanInstanceFormProps {
    onSubmit: (formula: string) => Promise<void>;
    loading: boolean;
    initialFormula?: string;
}

export function BooleanInstanceForm({onSubmit, loading, initialFormula = ''}: BooleanInstanceFormProps) {
    const [formula, setFormula] = useState(initialFormula);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async () => {
        if (!formula.trim()) {
            setError('Formula is required');
            return;
        }
        setError(null);
        await onSubmit(formula.trim());
    };

    return (
        <div className="space-y-4">
            <div>
                <label className="label">Boolean Formula</label>
                <textarea
                    className="input w-full font-geist-mono resize-y"
                    rows={4}
                    value={formula}
                    onChange={(e) => setFormula(e.target.value)}
                    placeholder="e.g. (x & y) | (x | !z)"
                    disabled={loading}
                />
                {error && <p className="text-error-500 text-xs mt-1">{error}</p>}
            </div>
            <div className="flex justify-end">
                <Button variant="primary" onClick={handleSubmit} disabled={loading}>
                    {loading && <Spinner size="sm"/>}
                    Set Instance
                </Button>
            </div>
        </div>
    );
}
