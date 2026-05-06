import {STATUS_COLOR} from '../lib/constants';
import type {ProblemSummaryResponse} from '../services/combinatorics';

interface ProblemCardProps {
    problem: ProblemSummaryResponse;
    active: boolean;
    onClick: () => void;
}

export function ProblemCard({problem, active, onClick}: ProblemCardProps) {
    const color = STATUS_COLOR[problem.solvingStatus] ?? '#9CA3AF';

    return (
        <button
            onClick={onClick}
            className={[
                'w-full text-left px-3 py-2.5 border-b border-neutral-100 dark:border-neutral-800',
                'transition-colors cursor-pointer relative',
                active
                    ? 'bg-primary-50 dark:bg-primary-900/20'
                    : 'bg-white dark:bg-surface-secondary hover:bg-neutral-50 dark:hover:bg-surface-tertiary',
            ].join(' ')}
        >
            {/* Left status stripe */}
            <span
                className="absolute left-0 top-0 bottom-0 w-[3px]"
                style={{backgroundColor: active ? '#0066CC' : color}}
            />
            <p className="text-sm font-medium text-neutral-900 dark:text-neutral-100 truncate pl-1">
                {problem.name}
            </p>
            <div className="flex items-center gap-1.5 mt-0.5 pl-1">
        <span
            className="inline-block w-1.5 h-1.5 rounded-full shrink-0"
            style={{backgroundColor: color}}
        />
                <span className="text-xs" style={{color}}>
          {problem.solvingStatus}
        </span>
                {problem.instanceType && (
                    <span className="text-xs text-neutral-400 dark:text-neutral-500 truncate">
            · {problem.instanceType}
          </span>
                )}
            </div>
        </button>
    );
}
