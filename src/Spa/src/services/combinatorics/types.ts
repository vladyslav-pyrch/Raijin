// ─── Shared / Primitives ─────────────────────────────────────────────────────

/**
 * Satisfiability result from the solver.
 * 0 = Unknown, 1 = Satisfiable, 2 = Unsatisfiable
 */
export const Satisfiability = {
  Unknown: 0,
  Satisfiable: 1,
  Unsatisfiable: 2,
} as const;
export type Satisfiability = (typeof Satisfiability)[keyof typeof Satisfiability];

// ─── Error types ─────────────────────────────────────────────────────────────

export interface ProblemDetails {
  type?: string | null;
  title?: string | null;
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
}

export interface HttpValidationProblemDetails extends ProblemDetails {
  errors?: Record<string, string[]>;
}

// ─── Problem ─────────────────────────────────────────────────────────────────

export interface GetProblemResponse {
  id: string;
  name: string;
  description: string;
  solver: string | null;
  instanceType: string | null;
  solvingStatus: string;
  satisfiability: string;
  createdAt: string;
  updatedAt: string;
  completedAt: string | null;
}

export interface ProblemSummaryResponse {
  id: string;
  name: string;
  instanceType: string | null;
  solvingStatus: string;
  satisfiability: string;
  createdAt: string;
}

export interface ListProblemsResponse {
  items: ProblemSummaryResponse[];
  page: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
}

export interface GetSatEncodingResponse {
  numberOfVariables: number;
  numberOfClauses: number;
  clauses: number[][];
}

export interface VariableMapEntryResponse {
  name: string;
  index: number;
}

export interface GetSatEncodingVariableMapResponse {
  variables: VariableMapEntryResponse[];
}

export interface CreateProblemRequest {
  name?: string;
  description?: string | null;
}

export interface CreateProblemResponse {
  problemId: string;
}

export interface UpdateProblemRequest {
  name?: string | null;
  description?: string | null;
}

export interface ReduceToSatRequest {
  solver?: string;
}

// ─── Vertex Coloring ─────────────────────────────────────────────────────────

export interface VertexColoringEdgeDto {
  label: string;
  u: string;
  v: string;
}

export interface VertexColoringInstanceDto {
  vertices: string[];
  edges: VertexColoringEdgeDto[];
  colorCount: number;
}

export interface VertexColorAssignmentDto {
  vertexId: string;
  color: number;
}

export interface VertexColoringSolutionDto {
  colorAssignments: VertexColorAssignmentDto[];
}

export interface GetVertexColoringSolutionResponse {
  solution: VertexColoringSolutionDto | null;
  satisfiability: Satisfiability;
}

export interface SetVertexColoringProblemInstanceRequest {
  instance?: VertexColoringInstanceDto;
}

// ─── Edge Coloring ───────────────────────────────────────────────────────────

export interface EdgeColoringEdgeDto {
  label: string;
  u: string;
  v: string;
}

export interface EdgeColoringInstanceDto {
  vertices: string[];
  edges: EdgeColoringEdgeDto[];
  colorCount: number;
}

export interface EdgeColorAssignmentDto {
  edgeLabel: string;
  color: number;
}

export interface EdgeColoringSolutionDto {
  colorAssignments: EdgeColorAssignmentDto[];
}

export interface GetEdgeColoringSolutionResponse {
  solution: EdgeColoringSolutionDto | null;
  satisfiability: Satisfiability;
}

export interface SetEdgeColoringProblemInstanceRequest {
  instance?: EdgeColoringInstanceDto;
}

// ─── CSP ─────────────────────────────────────────────────────────────────────

export interface DecisionVariableDto {
  name: string;
  states: string[];
}

export interface CspInstanceDto {
  variables: DecisionVariableDto[];
  constraints: string[];
}

export interface DecisionVariableStateAssignmentDto {
  name: string;
  assignedState: string;
}

export interface CspBooleanVariableAssignmentDto {
  variableName: string;
  value: boolean;
}

export interface CspSolutionDto {
  configuration: DecisionVariableStateAssignmentDto[];
  auxiliaryAssignments: CspBooleanVariableAssignmentDto[];
}

export interface GetCspSolutionResponse {
  solution: CspSolutionDto | null;
  satisfiability: Satisfiability;
}

export interface SetCspProblemInstanceRequest {
  instance?: CspInstanceDto;
}

// ─── Boolean ─────────────────────────────────────────────────────────────────

export interface BooleanProblemInstanceDto {
  formula: string;
}

export interface BooleanVariableAssignmentDto {
  variableName: string;
  value: boolean;
}

export interface BooleanSolutionDto {
  assignments: BooleanVariableAssignmentDto[];
}

export interface GetBooleanSolutionResponse {
  solution: BooleanSolutionDto | null;
  satisfiability: Satisfiability;
}

export interface SetBooleanProblemInstanceRequest {
  instance?: BooleanProblemInstanceDto;
}

// ─── Boolean Satisfiability (SAT) ────────────────────────────────────────────

export interface BooleanSatisfiabilityInstanceDto {
  /** Each inner array is a clause; each string is a literal name, e.g. "x1" or "~y2" (negated). */
  clauses: string[][];
}

/** GET /problems/{id}/instance/sat — clauses as literal strings, e.g. ["x1", "~y2"] */
export interface GetSatInstanceResponse {
  clauses: string[][];
}

export interface SatVariableAssignmentDto {
  variableName: string;
  value: boolean;
}

export interface BooleanSatisfiabilitySolutionDto {
  assignments: SatVariableAssignmentDto[];
}

export interface GetBooleanSatisfiabilitySolutionResponse {
  solution: BooleanSatisfiabilitySolutionDto | null;
  satisfiability: Satisfiability;
}

export interface SetBooleanSatisfiabilityProblemInstanceRequest {
  instance?: BooleanSatisfiabilityInstanceDto;
}
