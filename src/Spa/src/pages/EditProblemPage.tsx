import {useEffect, useState} from 'react';
import {useNavigate, useParams} from 'react-router-dom';
import {api} from '../lib/api';
import {useProblem} from '../hooks/useProblem';
import {Spinner} from '../components/Spinner';
import {ErrorStack, useErrorStack} from '../components/ErrorStack';

interface EditProblemPageProps {
  onProblemChanged: () => void;
}

export function EditProblemPage({ onProblemChanged }: EditProblemPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const problemId = id ?? '';

  const { problem, loading: problemLoading, error: problemError } = useProblem(problemId);

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [saving, setSaving] = useState(false);
  const { errors, addError, dismiss } = useErrorStack();

  // Pre-fill once problem data arrives
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
      await api.updateProblem(problemId, { name: name.trim(), description: description || null });
      onProblemChanged();
      navigate(`/problems/${problemId}`);
    } catch (err) {
      addError(err instanceof Error ? err.message : 'Update failed');
    } finally {
      setSaving(false);
    }
  };

  // ── Loading / error ───────────────────────────────────────────────────────

  if (problemLoading) {
    return (
      <div className="flex items-center justify-center h-full gap-3" style={{ color: '#545b64' }}>
        <Spinner size="lg" /> Loading…
      </div>
    );
  }

  if (problemError || !problem) {
    return (
      <div className="flex items-center justify-center h-full">
        <p className="text-sm" style={{ color: '#d13212' }}>
          {problemError ?? 'Problem not found'}
        </p>
      </div>
    );
  }

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <div className="flex flex-col h-full">
      {/* Page header */}
      <div
        className="shrink-0 border-b px-6 py-4"
        style={{ background: '#ffffff', borderColor: '#d5dbdb' }}
      >
        <p className="text-xs mb-1" style={{ color: '#879596' }}>
          Problems <span className="mx-1">›</span>
          <span
            className="cursor-pointer hover:underline"
            style={{ color: '#0073bb' }}
            onClick={() => navigate(`/problems/${problemId}`)}
          >
            {problem.name}
          </span>
          <span className="mx-1">›</span>
          <span style={{ color: '#0073bb' }}>Edit</span>
        </p>
        <div className="flex items-center justify-between">
          <h1 className="text-lg font-semibold" style={{ color: '#16191f' }}>
            Edit problem
          </h1>
          <button
            onClick={() => navigate(`/problems/${problemId}`)}
            className="rounded border px-3 py-1.5 text-sm cursor-pointer"
            style={{ borderColor: '#aab7b8', color: '#545b64', background: '#fff' }}
          >
            Cancel
          </button>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto px-6 py-5">
        <div className="w-full space-y-5">
          {/* Errors */}
          <ErrorStack errors={errors} onDismiss={dismiss} />

          {/* Name */}
          <div>
            <label className="block text-xs font-semibold mb-1" style={{ color: '#16191f' }}>
              Name <span style={{ color: '#d13212' }}>*</span>
            </label>
            <input
              className="w-full rounded border px-3 py-2 text-sm focus:outline-none focus:ring-1"
              style={{ borderColor: '#aab7b8', color: '#16191f' }}
              value={name}
              onChange={(e) => setName(e.target.value)}
              disabled={saving}
              placeholder="Problem name"
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-xs font-semibold mb-1" style={{ color: '#16191f' }}>
              Description
            </label>
            <textarea
              className="w-full rounded border px-3 py-2 text-sm focus:outline-none focus:ring-1"
              style={{ borderColor: '#aab7b8', color: '#16191f' }}
              rows={5}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              disabled={saving}
              placeholder="Optional description…"
            />
          </div>

          {/* Actions */}
          <div className="flex justify-end">
            <button
              onClick={handleSave}
              disabled={saving}
              className="rounded px-5 py-2 text-sm font-semibold cursor-pointer disabled:opacity-50"
              style={{ background: '#ff9900', color: '#16191f', border: '1px solid #e88b00' }}
            >
              {saving ? 'Saving…' : 'Save changes'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
