import {useState} from 'react';
import {Button} from '../Button';
import {Spinner} from '../Spinner';

interface SatInstanceFormProps {
    onSubmit: (clauses: string[][]) => Promise<void>;
    onDimacsSubmit?: (file: File) => Promise<void>;
    loading: boolean;
    initialText?: string;
}

type SatInputMode = 'manual' | 'file';

export function SatInstanceForm({onSubmit, onDimacsSubmit, loading, initialText = ''}: SatInstanceFormProps) {
    const [text, setText] = useState(initialText);
    const [inputMode, setInputMode] = useState<SatInputMode>('manual');
    const [file, setFile] = useState<File | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async () => {
        setError(null);

        if (inputMode === 'file') {
            if (!onDimacsSubmit) {
                setError('File upload is not available here');
                return;
            }

            if (!file) {
                setError('Choose a .cnf file');
                return;
            }

            if (!file.name.toLowerCase().endsWith('.cnf')) {
                setError('SAT DIMACS file must use .cnf extension');
                return;
            }

            await onDimacsSubmit(file);
            return;
        }

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
            {onDimacsSubmit && (
                <div className="inline-flex rounded-md border border-neutral-200 dark:border-neutral-700 bg-white dark:bg-surface-secondary p-1">
                    {(['manual', 'file'] as SatInputMode[]).map((mode) => (
                        <button
                            key={mode}
                            type="button"
                            onClick={() => {
                                setInputMode(mode);
                                setError(null);
                            }}
                            className={`btn btn-sm capitalize ${inputMode === mode ? 'btn-primary' : 'btn-ghost'}`}
                            disabled={loading}
                        >
                            {mode === 'manual' ? 'Manual' : 'File'}
                        </button>
                    ))}
                </div>
            )}

            {inputMode === 'manual' ? (
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
                    <p className="text-neutral-400 dark:text-neutral-500 text-xs mt-1">
                        Each line is a clause. Variable name = positive literal.{' '}
                        <code
                            className="font-geist-mono bg-neutral-100 dark:bg-surface-tertiary px-1 rounded">~name</code>{' '}
                        = negation.
                    </p>
                </div>
            ) : (
                <div>
                    <label className="label">DIMACS CNF file</label>
                    <input
                        type="file"
                        accept=".cnf"
                        className="input w-full cursor-pointer"
                        onChange={(e) => setFile(e.target.files?.[0] ?? null)}
                        disabled={loading}
                    />
                    <p className="text-neutral-400 dark:text-neutral-500 text-xs mt-1">
                        Upload a DIMACS CNF file with <code
                        className="font-geist-mono bg-neutral-100 dark:bg-surface-tertiary px-1 rounded">.cnf</code> extension.
                    </p>
                </div>
            )}

            {error && <p className="text-error-500 text-xs">{error}</p>}

            <div className="flex justify-end">
                <Button variant="primary" onClick={handleSubmit} disabled={loading}>
                    {loading && <Spinner size="sm"/>}
                    {inputMode === 'file' ? 'Upload File' : 'Set Instance'}
                </Button>
            </div>
        </div>
    );
}
