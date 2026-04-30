import {useNavigate, useParams} from 'react-router-dom';
import {useProblem} from '../hooks/useProblem';
import {useSatEncoding} from '../hooks/useSatEncoding';
import {useInstance} from '../hooks/useInstance';
import {api} from '../lib/api';
import type {InstanceTypeValue} from '../lib/constants';
import {SOLVER_OPTIONS, STATUSES_WITH_ENCODING} from '../lib/constants';
import {DropdownButton} from '../components/DropdownButton';
import {Spinner} from '../components/Spinner';
import {StatusBadge} from '../components/StatusBadge';
import {ErrorStack, useErrorStack} from '../components/ErrorStack';
import type {ForkPrefill} from './CreateProblemPage';
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

  const { instance, loading: instLoading, error: instError } = useInstance(
    problemId,
    problem?.instanceType ?? null,
  );

  // Errors shown as stacked red blocks
  const { errors, addError, dismiss } = useErrorStack();

  // ── Handlers ──────────────────────────────────────────────────────────────

  const handleRefresh = () => {
    refresh();
    onProblemChanged();
  };

  const handleSolve = async (solver: string) => {
    try {
      await api.solve(problemId, solver);
      refresh();
      onProblemChanged();
    } catch (err) {
      addError(err instanceof Error ? err.message : 'Solve failed');
    }
  };

  const handleEdit = () => {
    navigate(`/problems/${problemId}/edit`);
  };

  const handleNewVersion = () => {
    if (!instance || !problem?.instanceType) return;
    const fork: ForkPrefill = {
      name: problem.name,
      description: problem.description ?? '',
      instanceType: problem.instanceType as InstanceTypeValue,
      instance,
    };
    navigate('/create', { state: { fork } });
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
  const canFork = !!instance && !!problem.instanceType && !instLoading;

  return (
    <div className="flex flex-col h-full">
      {/* Page header */}
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
            <button
              onClick={handleRefresh}
              className="rounded border px-3 py-1.5 text-xs font-medium cursor-pointer transition-colors"
              style={{ borderColor: '#aab7b8', color: '#545b64', background: '#fff' }}
              onMouseEnter={(e) => (e.currentTarget.style.background = '#f2f3f3')}
              onMouseLeave={(e) => (e.currentTarget.style.background = '#fff')}
            >
              ↻ Check the Problem Status
            </button>
            <button
              onClick={handleEdit}
              className="rounded border px-3 py-1.5 text-xs font-medium cursor-pointer transition-colors"
              style={{ borderColor: '#aab7b8', color: '#545b64', background: '#fff' }}
              onMouseEnter={(e) => (e.currentTarget.style.background = '#f2f3f3')}
              onMouseLeave={(e) => (e.currentTarget.style.background = '#fff')}
            >
              Edit
            </button>
            {canFork && (
              <button
                onClick={handleNewVersion}
                className="rounded border px-3 py-1.5 text-xs font-medium cursor-pointer transition-colors"
                style={{ borderColor: '#aab7b8', color: '#545b64', background: '#fff' }}
                onMouseEnter={(e) => (e.currentTarget.style.background = '#f2f3f3')}
                onMouseLeave={(e) => (e.currentTarget.style.background = '#fff')}
              >
                New version
              </button>
            )}
            {canSolve && (
              <DropdownButton
                label="Solve"
                options={solverOptions}
                onSelect={handleSolve}
              />
            )}
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto px-6 py-5 space-y-4">
        {/* Stacked error blocks */}
        <ErrorStack errors={errors} onDismiss={dismiss} />

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
          instance={instance}
        />
      </div>
    </div>
  );
}
