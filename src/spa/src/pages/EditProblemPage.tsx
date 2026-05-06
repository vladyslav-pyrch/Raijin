import {useEffect, useState} from 'react';
import {useNavigate, useParams} from 'react-router-dom';
import {api} from '../lib/api';
import {useProblem} from '../hooks/useProblem';
import {Spinner} from '../components/Spinner';
import {ErrorStack, useErrorStack} from '../components/ErrorStack';

interface EditProblemPageProps {
    onProblemChanged: () => void;
}

export function EditProblemPage({onProblemChanged}: EditProblemPageProps) {
    const {id} = useParams<{ id: string }>();
    const navigate = useNavigate();
    const problemId = id ?? '';

    const {problem, loading: problemLoading, error: problemError} = useProblem(problemId);

    const [name, setName] = useState('');
    const [description, setDescription] = useState('');
    const [saving, setSaving] = useState(false);
    const {errors, addError, dismiss} = useErrorStack();

    useEffect(() => {
        if (problem) {
            setName(problem.name);
            setDescription(problem.description ?? '');
        }
    }, [problem]);

    const handleSave = async () => {
        if (!name.trim()) {
            addError('Name is required');
            return;
        }
        setSaving(true);
        try {
            await api.updateProblem(problemId, {name: name.trim(), description: description || null});
            onProblemChanged();
            navigate(`/problems/${problemId}`);
        } catch (err) {
            addError(err instanceof Error ? err.message : 'Update failed');
        } finally {
            setSaving(false);
        }
    };

    if (problemLoading) {
        return (
            <div className="flex items-center justify-center h-full gap-3 text-neutral-500 dark:text-neutral-400">
                <Spinner size="lg"/> Loading…
            </div>
        );
    }

    if (problemError || !problem) {
        return (
            <div className="flex items-center justify-center h-full">
                <p className="text-sm text-error-500">{problemError ?? 'Problem not found'}</p>
            </div>
        );
    }

    return (
        <div className="flex flex-col h-full">
            {/* Page header */}
            <div
                className="shrink-0 border-b border-neutral-200 dark:border-neutral-700 px-6 py-4 bg-white dark:bg-surface-secondary">
                <p className="text-xs mb-1 text-neutral-400 dark:text-neutral-500">
                    Problems <span className="mx-1">›</span>
                    <button
                        className="link"
                        onClick={() => navigate(`/problems/${problemId}`)}
                    >
                        {problem.name}
                    </button>
                    <span className="mx-1">›</span>
                    <span className="text-primary-500 dark:text-primary-400">Edit</span>
                </p>
                <div className="flex items-center justify-between">
                    <h1 className="text-lg font-semibold text-neutral-900 dark:text-neutral-100">
                        Edit problem
                    </h1>
                    <button
                        onClick={() => navigate(`/problems/${problemId}`)}
                        className="btn btn-secondary btn-sm"
                    >
                        Cancel
                    </button>
                </div>
            </div>

            {/* Content */}
            <div className="flex-1 overflow-y-auto px-6 py-5">
                <div className="w-full space-y-5">
                    <ErrorStack errors={errors} onDismiss={dismiss}/>

                    <div>
                        <label className="label">
                            Name <span className="text-error-500">*</span>
                        </label>
                        <input
                            className="input w-full"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            disabled={saving}
                            placeholder="Problem name"
                        />
                    </div>

                    <div>
                        <label className="label">Description</label>
                        <textarea
                            className="input w-full resize-y"
                            rows={5}
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            disabled={saving}
                            placeholder="Optional description…"
                        />
                    </div>

                    <div className="flex justify-end">
                        <button
                            onClick={handleSave}
                            disabled={saving}
                            className="btn btn-primary disabled:opacity-50"
                        >
                            {saving && <Spinner size="sm"/>}
                            {saving ? 'Saving…' : 'Save changes'}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}
