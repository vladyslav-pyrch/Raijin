import {StatusBadge} from '../StatusBadge';
import type {GetProblemResponse} from '../../services/combinatorics';

function Row({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div
      className="flex py-2 border-b last:border-0"
      style={{ borderColor: '#eaeded' }}
    >
      <span
        className="w-40 shrink-0 text-xs font-medium"
        style={{ color: '#545b64' }}
      >
        {label}
      </span>
      <span className="text-sm flex-1 min-w-0 break-all" style={{ color: '#16191f' }}>
        {children}
      </span>
    </div>
  );
}

function fmt(dateStr: string | null | undefined) {
  if (!dateStr) return <span style={{ color: '#879596' }}>—</span>;
  return new Date(dateStr).toLocaleString();
}

interface ProblemInfoSectionProps {
  problem: GetProblemResponse;
}

export function ProblemInfoSection({ problem }: ProblemInfoSectionProps) {
  return (
    <section className="bg-white border rounded" style={{ borderColor: '#d5dbdb' }}>
      <div className="px-4 py-3 border-b" style={{ borderColor: '#d5dbdb', background: '#fafafa' }}>
        <h2 className="text-sm font-semibold" style={{ color: '#16191f' }}>
          Problem details
        </h2>
      </div>
      <div className="px-4 py-2">
        <Row label="ID">
          <code className="text-xs font-mono px-1.5 py-0.5 rounded" style={{ background: '#f2f3f3', color: '#545b64' }}>
            {problem.id}
          </code>
        </Row>
        <Row label="Instance type">
          {problem.instanceType ?? <span style={{ color: '#879596' }}>—</span>}
        </Row>
        <Row label="Solving status">
          <StatusBadge status={problem.solvingStatus} />
        </Row>
        <Row label="Satisfiability">{problem.satisfiability}</Row>
        <Row label="Solver">{problem.solver ?? <span style={{ color: '#879596' }}>—</span>}</Row>
        <Row label="Created">{fmt(problem.createdAt)}</Row>
        <Row label="Updated">{fmt(problem.updatedAt)}</Row>
        <Row label="Completed">{fmt(problem.completedAt)}</Row>
      </div>
    </section>
  );
}
