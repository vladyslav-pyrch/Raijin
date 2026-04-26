import {useState} from 'react';
import {useNavigate, useParams} from 'react-router-dom';
import {useProblem} from '../hooks/useProblem';
import {useSatEncoding} from '../hooks/useSatEncoding';
import {useInstance} from '../hooks/useInstance';
import {api} from '../lib/api';
import type {InstanceTypeValue} from '../lib/constants';
import {SOLVER_OPTIONS, STATUSES_WITH_ENCODING} from '../lib/constants';
import {Button} from '../components/Button';
import {DropdownButton} from '../components/DropdownButton';
import {Modal} from '../components/Modal';
import {Spinner} from '../components/Spinner';
import {StatusBadge} from '../components/StatusBadge';
import {type ForkPrefill, SetInstanceModal} from '../components/forms/SetInstanceModal';
import {ProblemInfoSection} from '../components/sections/ProblemInfoSection';
import {DescriptionSection} from '../components/sections/DescriptionSection';
import {InstanceSection} from '../components/sections/InstanceSection';
import {SatEncodingSection} from '../components/sections/SatEncodingSection';
import {VariableMapSection} from '../components/sections/VariableMapSection';
import {SolutionSection} from '../components/sections/SolutionSection';

interface ProblemDetailPageProps {
  onProblemChanged: () => void;
}

export function ProblemDetailPage({ onProblemChanged }: ProblemDetailPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const problemId = id ?? '';

  const { problem, loading, error, refresh } = useProblem(problemId);

  const hasEncoding = STATUSES_WITH_ENCODING.includes(
    problem?.solvingStatus as (typeof STATUSES_WITH_ENCODING)[number],
  );
  const canSolve = problem?.solvingStatus !== 'Running' && problem?.instanceType !== null;

  const { encoding, loading: encLoading, error: encError } = useSatEncoding(problemId, hasEncoding);

  // Lift instance data here so Edit (fork) modal can pre-fill it
  const { instance, loading: instLoading, error: instError } = useInstance(
    problemId,
    problem?.instanceType ?? null,
  );

  // modals
  const [updateOpen, setUpdateOpen] = useState(false);
  const [forkOpen, setForkOpen] = useState(false);

  // update form
  const [updName, setUpdName] = useState('');
  const [updDesc, setUpdDesc] = useState('');
  const [updLoading, setUpdLoading] = useState(false);
  const [updError, setUpdError] = useState<string | null>(null);

  const openUpdate = () => {
    setUpdName(problem?.name ?? '');
    setUpdDesc(problem?.description ?? '');
    setUpdError(null);
    setUpdateOpen(true);
  };

  const handleUpdate = async () => {
    setUpdLoading(true);
    setUpdError(null);
    try {
      await api.updateProblem(problemId, { name: updName || null, description: updDesc || null });
      setUpdateOpen(false);
      refresh();
      onProblemChanged();
    } catch (err) {
      setUpdError(err instanceof Error ? err.message : 'Update failed');
    } finally {
      setUpdLoading(false);
    }
  };

  // solve
  const [solveLoading, setSolveLoading] = useState(false);
  const [solveError, setSolveError] = useState<string | null>(null);

  const handleSolve = async (solver: string) => {
    setSolveLoading(true);
    setSolveError(null);
    try {
      await api.solve(problemId, solver);
      refresh();
      onProblemChanged();
    } catch (err) {
      setSolveError(err instanceof Error ? err.message : 'Solve failed');
    } finally {
      setSolveLoading(false);
    }
  };

  // ── Loading / error states ────────────────────────────────────────────────
  if (loading) {
    return (
      <div className="flex items-center justify-center h-full gap-3" style={{ color: '#545b64' }}>
        <Spinner size="lg" /> Loading…
      </div>
    );
  }

  if (error || !problem) {
    return (
      <div className="flex items-center justify-center h-full">
        <p className="text-sm" style={{ color: '#d13212' }}>
          {error ?? 'Problem not found'}
        </p>
      </div>
    );
  }

  const solverOptions = SOLVER_OPTIONS.map((s) => ({ label: s, value: s }));

  // Build fork prefill when instance data is available
  const forkPrefill: ForkPrefill | undefined =
    instance && problem.instanceType
      ? {
          name: problem.name,
          description: problem.description ?? '',
          instanceType: problem.instanceType as InstanceTypeValue,
          instance,
        }
      : undefined;

  return (
    <div className="flex flex-col h-full">
      {/* ── Page header ─────────────────────────────────────────────────── */}
      <div
        className="shrink-0 border-b px-6 py-4"
        style={{ background: '#ffffff', borderColor: '#d5dbdb' }}
      >
        {/* Breadcrumb */}
        <p className="text-xs mb-1" style={{ color: '#879596' }}>
          Problems <span className="mx-1">›</span>
          <span style={{ color: '#0073bb' }}>{problem.name}</span>
        </p>

        {/* Title row */}
        <div className="flex items-start justify-between gap-4">
          <div className="min-w-0">
            <h1 className="text-lg font-semibold truncate" style={{ color: '#16191f' }}>
              {problem.name}
            </h1>
            <div className="mt-1">
              <StatusBadge status={problem.solvingStatus} />
            </div>
          </div>

          {/* Action bar */}
          <div className="flex items-center gap-2 shrink-0 flex-wrap justify-end">
            <Button variant="secondary" size="sm" onClick={() => { refresh(); onProblemChanged(); }}>
              ↻ Refresh
            </Button>
            <Button variant="secondary" size="sm" onClick={openUpdate}>
              Edit
            </Button>
            {/* Fork: create new versioned problem pre-filled with current data */}
            {forkPrefill && (
              <Button
                variant="secondary"
                size="sm"
                onClick={() => setForkOpen(true)}
                disabled={instLoading}
              >
                New version
              </Button>
            )}
            {canSolve && (
              <DropdownButton
                label="Solve"
                options={solverOptions}
                onSelect={handleSolve}
                loading={solveLoading}
              />
            )}
          </div>
        </div>

        {solveError && (
          <p className="text-xs mt-2" style={{ color: '#d13212' }}>
            {solveError}
          </p>
        )}
      </div>

      {/* ── Content ─────────────────────────────────────────────────────── */}
      <div className="flex-1 overflow-y-auto px-6 py-5 space-y-4">
        <ProblemInfoSection problem={problem} />
        <DescriptionSection description={problem.description} />
        <InstanceSection
          instanceType={problem.instanceType}
          instance={instance}
          loading={instLoading}
          error={instError}
        />

        {hasEncoding && (
          <SatEncodingSection encoding={encoding} loading={encLoading} error={encError} />
        )}

        {hasEncoding && <VariableMapSection key={problemId} problemId={problemId} />}

        <SolutionSection
          key={`sol-${problemId}`}
          problemId={problemId}
          instanceType={problem.instanceType}
          solvingStatus={problem.solvingStatus}
        />
      </div>

      {/* ── Edit (name/description only) modal ──────────────────────────── */}
      <Modal open={updateOpen} title="Edit problem" onClose={() => setUpdateOpen(false)}>
        <div className="space-y-4">
          <div>
            <label className="block text-xs font-semibold mb-1" style={{ color: '#16191f' }}>
              Name
            </label>
            <input
              className="w-full border rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-1"
              style={{ borderColor: '#aab7b8', color: '#16191f' }}
              value={updName}
              onChange={(e) => setUpdName(e.target.value)}
              disabled={updLoading}
            />
          </div>
          <div>
            <label className="block text-xs font-semibold mb-1" style={{ color: '#16191f' }}>
              Description
            </label>
            <textarea
              className="w-full border rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-1"
              style={{ borderColor: '#aab7b8', color: '#16191f' }}
              rows={4}
              value={updDesc}
              onChange={(e) => setUpdDesc(e.target.value)}
              disabled={updLoading}
            />
          </div>
          {updError && (
            <p className="text-xs" style={{ color: '#d13212' }}>
              {updError}
            </p>
          )}
          <div className="flex justify-end gap-2 pt-1">
            <Button variant="secondary" onClick={() => setUpdateOpen(false)} disabled={updLoading}>
              Cancel
            </Button>
            <Button variant="primary" onClick={handleUpdate} disabled={updLoading}>
              {updLoading && <Spinner size="sm" />}
              Save changes
            </Button>
          </div>
        </div>
      </Modal>

      {/* ── Fork (new version) modal ─────────────────────────────────────── */}
      {forkPrefill && (
        <SetInstanceModal
          open={forkOpen}
          onClose={() => setForkOpen(false)}
          onSuccess={async (newId) => {
            setForkOpen(false);
            await onProblemChanged();
            navigate(`/problems/${newId}`);
          }}
          prefill={forkPrefill}
        />
      )}
    </div>
  );
}
