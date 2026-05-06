import {StatusBadge} from '../StatusBadge';
import type {GetProblemResponse} from '../../services/combinatorics';

function Row({label, children}: { label: string; children: React.ReactNode }) {
    return (
        <div className="flex py-2 border-b border-neutral-100 dark:border-neutral-800 last:border-0">
      <span className="w-40 shrink-0 text-xs font-medium text-neutral-500 dark:text-neutral-400">
        {label}
      </span>
            <span className="text-sm flex-1 min-w-0 break-all text-neutral-900 dark:text-neutral-100">
        {children}
      </span>
        </div>
    );
}

function fmt(dateStr: string | null | undefined) {
    if (!dateStr) return <span className="text-neutral-400 dark:text-neutral-500">—</span>;
    return new Date(dateStr).toLocaleString();
}

interface ProblemInfoSectionProps {
    problem: GetProblemResponse;
}

export function ProblemInfoSection({problem}: ProblemInfoSectionProps) {
    return (
        <section className="card">
            <div className="card-header">
                <h2 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">
                    Problem details
                </h2>
            </div>
            <div className="px-4 py-2">
                <Row label="ID">
                    <code
                        className="text-xs font-geist-mono px-1.5 py-0.5 rounded bg-neutral-100 dark:bg-surface-tertiary text-neutral-600 dark:text-neutral-300">
                        {problem.id}
                    </code>
                </Row>
                <Row label="Instance type">
                    {problem.instanceType ?? <span className="text-neutral-400 dark:text-neutral-500">—</span>}
                </Row>
                <Row label="Solving status">
                    <StatusBadge status={problem.solvingStatus}/>
                </Row>
                <Row label="Satisfiability">{problem.satisfiability}</Row>
                <Row label="Solver">
                    {problem.solver ?? <span className="text-neutral-400 dark:text-neutral-500">—</span>}
                </Row>
                <Row label="Created">{fmt(problem.createdAt)}</Row>
                <Row label="Updated">{fmt(problem.updatedAt)}</Row>
                <Row label="Completed">{fmt(problem.completedAt)}</Row>
            </div>
        </section>
    );
}
