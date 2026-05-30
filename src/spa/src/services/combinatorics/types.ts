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
    startedSolvingAt: string | null;
    completedAt: string | null;
    elapsedTime: string | null;
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

export interface UpdateProblemRequest {
    name?: string | null;
    description?: string | null;
}

export type DeleteProblemResponse = void;

// ─── Graph primitives ─────────────────────────────────────────────────────────

export interface VertexDto {
    id: string;
}

export interface EdgeDto {
    label: string;
    u: string;
    v: string;
}

export interface GraphDto {
    vertices: VertexDto[];
    edges: EdgeDto[];
}

// ─── Vertex Coloring ─────────────────────────────────────────────────────────

export interface VertexColoringInstanceDto {
    graph: GraphDto;
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

export interface GetVertexColoringInstanceResponse {
    instance: VertexColoringInstanceDto;
}

export interface CreateVertexColoringProblemRequest {
    name?: string;
    description?: string | null;
    instance?: VertexColoringInstanceDto;
}

export interface CreateVertexColoringProblemFromDimacsRequest {
    name?: string;
    description?: string | null;
    colorCount: number;
    file: File;
}

export interface CreateVertexColoringProblemResponse {
    problemId: string;
}

// ─── Edge Coloring ───────────────────────────────────────────────────────────

export interface EdgeColoringInstanceDto {
    graph: GraphDto;
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

export interface GetEdgeColoringInstanceResponse {
    instance: EdgeColoringInstanceDto;
}

export interface CreateEdgeColoringProblemRequest {
    name?: string;
    description?: string | null;
    instance?: EdgeColoringInstanceDto;
}

export interface CreateEdgeColoringProblemFromDimacsRequest {
    name?: string;
    description?: string | null;
    colorCount: number;
    file: File;
}

export interface CreateEdgeColoringProblemResponse {
    problemId: string;
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

export interface GetCspInstanceResponse {
    instance: CspInstanceDto;
}

export interface CreateCspProblemRequest {
    name?: string;
    description?: string | null;
    instance?: CspInstanceDto;
}

export interface CreateCspProblemResponse {
    problemId: string;
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

export interface GetBooleanInstanceResponse {
    instance: BooleanProblemInstanceDto;
}

export interface CreateBooleanProblemRequest {
    name?: string;
    description?: string | null;
    instance?: BooleanProblemInstanceDto;
}

export interface CreateBooleanProblemResponse {
    problemId: string;
}

// ─── Boolean Satisfiability (SAT) ────────────────────────────────────────────

/** Each inner array is a clause; each string is a literal name, e.g. "x1" or "~y2" (negated). */
export interface SatInstanceDto {
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

export interface GetBooleanSatisfiabilityInstanceResponse {
    instance: SatInstanceDto;
}

export interface CreateSatProblemRequest {
    name?: string;
    description?: string | null;
    instance?: SatInstanceDto;
}

export interface CreateSatProblemFromDimacsRequest {
    name?: string;
    description?: string | null;
    file: File;
}

export interface CreateSatProblemResponse {
    problemId: string;
}
