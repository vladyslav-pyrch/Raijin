import {STATUS_COLOR} from '../lib/constants';
import type {ProblemSummaryResponse} from '../services/combinatorics';

interface ProblemCardProps {
  problem: ProblemSummaryResponse;
  active: boolean;
  onClick: () => void;
}

export function ProblemCard({ problem, active, onClick }: ProblemCardProps) {
  const color = STATUS_COLOR[problem.solvingStatus] ?? '#879596';

  return (
    <button
      onClick={onClick}
      className={[
        'w-full text-left px-3 py-2.5 border-b border-[#eaeded] transition-colors cursor-pointer relative',
        active
          ? 'bg-[#fef6e4]'
          : 'bg-white hover:bg-[#f2f3f3]',
      ].join(' ')}
    >
      {/* left status stripe */}
      <span
        className="absolute left-0 top-0 bottom-0 w-[3px]"
        style={{ backgroundColor: active ? '#ff9900' : color }}
      />
      <p className="text-sm font-medium text-[#16191f] truncate pl-1">{problem.name}</p>
      <div className="flex items-center gap-1.5 mt-0.5 pl-1">
        <span
          className="inline-block w-1.5 h-1.5 rounded-full shrink-0"
          style={{ backgroundColor: color }}
        />
        <span className="text-xs" style={{ color }}>
          {problem.solvingStatus}
        </span>
        {problem.instanceType && (
          <span className="text-xs text-[#879596] truncate">· {problem.instanceType}</span>
        )}
      </div>
    </button>
  );
}
