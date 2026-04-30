/** Claude Code design system status colors. */
export const STATUS_COLOR: Record<string, string> = {
  NoSatEncoding: '#9CA3AF',  // neutral-400
  Pending:       '#FF9933',  // warning-400
  Running:       '#0066CC',  // primary-500
  Completed:     '#10A760',  // success-500
  Failed:        '#DC3545',  // error-500
  TimedOut:      '#FF6D2A',  // warning-500
};

/** @deprecated Use STATUS_COLOR */
export const STATUS_BORDER_COLOR = STATUS_COLOR;

export const SOLVER_OPTIONS = ['cadical', 'cryptominisat'] as const;

export const STATUSES_WITH_ENCODING = [
  'Pending',
  'Running',
  'Completed',
  'Failed',
  'TimedOut',
] as const;

/**
 * Instance type values match backend ProblemTypes constants exactly:
 * "bool" | "sat" | "csp" | "vertex-coloring" | "edge-coloring"
 */
export const INSTANCE_TYPES = [
  { value: 'bool',             label: 'Boolean Expression' },
  { value: 'sat',              label: 'Boolean Satisfiability (SAT)' },
  { value: 'csp',              label: 'Constraint Satisfaction Problem (CSP)' },
  { value: 'vertex-coloring',  label: 'Vertex Coloring' },
  { value: 'edge-coloring',    label: 'Edge Coloring' },
] as const;

export type InstanceTypeValue = (typeof INSTANCE_TYPES)[number]['value'];
