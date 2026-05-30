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

function fmtTimeSpan(timeSpan: string | null | undefined) {
    if (!timeSpan) return <span className="text-neutral-400 dark:text-neutral-500">—</span>;
    
    const negative = timeSpan.startsWith("-");
    const raw = negative ? timeSpan.slice(1) : timeSpan;

    // Split off days if present (contains a '.' before the first ':')
    let days = 0;
    let timePart = raw;

    const dotIndex = raw.indexOf(".");
    const colonIndex = raw.indexOf(":");

    if (dotIndex !== -1 && (colonIndex === -1 || dotIndex < colonIndex)) {
        days = parseInt(raw.slice(0, dotIndex), 10);
        timePart = raw.slice(dotIndex + 1);
    }

    // timePart is now hh:mm:ss[.fffffff]
    const [hhmmss, fracStr = "0"] = timePart.split(".");
    const [hh, mm, ss] = hhmmss!.split(":").map(Number);

    // C# ticks = 100ns, 7 digits. Pad/truncate to 7 digits then take first 3 for ms
    const fracPadded = fracStr.padEnd(7, "0").slice(0, 7);
    const ms = Math.round(parseInt(fracPadded, 10) / 10_000); // ticks → ms

    type Unit = { value: number; label: string };

    const units: Unit[] = [
        { value: days, label: "d" },
        { value: hh!,   label: "h" },
        { value: mm!,   label: "m" },
        { value: ss!,   label: "s" },
        { value: ms,   label: "ms" },
    ];

    // Trim leading and trailing zero units, keep everything in between
    const firstNonZero = units.findIndex((u) => u.value !== 0);
    const lastNonZero  = units.findLastIndex((u) => u.value !== 0);

    if (firstNonZero === -1) return "0ms";

    const formatted = units
        .slice(firstNonZero, lastNonZero + 1)
        .map((u) => `${u.value}${u.label}`)
        .join(" ");

    return negative ? `-${formatted}` : formatted;
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
                <Row label="Started solving">{fmt(problem.startedSolvingAt)}</Row>
                <Row label="Completed">{fmt(problem.completedAt)}</Row>
                <Row label="Duration">{fmtTimeSpan(problem.elapsedTime)}</Row>
            </div>
        </section>
    );
}
