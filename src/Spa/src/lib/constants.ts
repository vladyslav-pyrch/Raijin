/** AWS-palette status colors — used for inline style (left stripe + badge). */
export const STATUS_COLOR: Record<string, string> = {
  NoSatEncoding: '#879596',
  Pending:       '#ff9900',
  Running:       '#0073bb',
  Completed:     '#1d8348',
  Failed:        '#d13212',
  TimedOut:      '#f5a623',
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
