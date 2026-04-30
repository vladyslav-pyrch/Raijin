import {STATUS_COLOR} from '../lib/constants';

export function StatusBadge({ status }: { status: string }) {
  const color = STATUS_COLOR[status] ?? '#9CA3AF';
  return (
    <span
      className="inline-flex items-center gap-1.5 text-xs font-medium"
      style={{ color }}
    >
      <span
        className="inline-block w-2 h-2 rounded-full shrink-0"
        style={{ backgroundColor: color }}
      />
      {status}
    </span>
  );
}
