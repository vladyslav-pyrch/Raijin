import type {
  BooleanProblemInstanceDto,
  CreateProblemRequest,
  CreateProblemResponse,
  CspInstanceDto,
  EdgeColoringInstanceDto,
  GetBooleanSatisfiabilitySolutionResponse,
  GetBooleanSolutionResponse,
  GetCspSolutionResponse,
  GetEdgeColoringSolutionResponse,
  GetProblemResponse,
  GetSatEncodingResponse,
  GetSatEncodingVariableMapResponse,
  GetSatInstanceResponse,
  GetVertexColoringSolutionResponse,
  HttpValidationProblemDetails,
  ListProblemsResponse,
  ProblemDetails,
  ReduceToSatRequest,
  SetBooleanProblemInstanceRequest,
  SetBooleanSatisfiabilityProblemInstanceRequest,
  SetCspProblemInstanceRequest,
  SetEdgeColoringProblemInstanceRequest,
  SetVertexColoringProblemInstanceRequest,
  UpdateProblemRequest,
  VertexColoringInstanceDto,
} from './types';

// ─── Error ───────────────────────────────────────────────────────────────────

export class ApiError extends Error {
  readonly status: number;
  readonly details: ProblemDetails | HttpValidationProblemDetails;

  constructor(
    status: number,
    details: ProblemDetails | HttpValidationProblemDetails,
  ) {
    super(details.detail ?? details.title ?? `HTTP ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

// ─── Service ─────────────────────────────────────────────────────────────────

export class CombinatoricsApiService {
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  // ── Core helper ────────────────────────────────────────────────────────────

  private async request<T = void>(
    method: string,
    path: string,
    body?: unknown,
  ): Promise<T> {
    const headers: Record<string, string> = {
      Accept: 'application/json',
    };

    if (body !== undefined) {
      headers['Content-Type'] = 'application/json';
    }

    const url = `${this.baseUrl.replace(/\/$/, '')}${path}`;

    const init: RequestInit = { method: method, headers: headers };
    if (body !== undefined) {
      init.body = JSON.stringify(body);
    }

    const response = await fetch(url, init);

    if (response.status === 204 || response.status === 201) {
      const text = await response.text();
      return (text ? (JSON.parse(text) as T) : undefined) as T;
    }

    if (!response.ok) {
      let details: ProblemDetails;
      try {
        details = (await response.json()) as ProblemDetails;
      } catch {
        details = { status: response.status, title: response.statusText };
      }
      throw new ApiError(response.status, details);
    }

    return response.json() as Promise<T>;
  }

  // ── Problems ───────────────────────────────────────────────────────────────

  getProblem(id: string): Promise<GetProblemResponse> {
    return this.request<GetProblemResponse>('GET', `/problems/${id}`);
  }

  listProblems(page = 1, pageSize = 20): Promise<ListProblemsResponse> {
    return this.request<ListProblemsResponse>(
      'GET',
      `/problems?page=${page}&pageSize=${pageSize}`,
    );
  }

  getSatEncoding(id: string): Promise<GetSatEncodingResponse> {
    return this.request<GetSatEncodingResponse>(
      'GET',
      `/problems/${id}/sat-encoding`,
    );
  }

  getSatEncodingVariableMap(id: string): Promise<GetSatEncodingVariableMapResponse> {
    return this.request<GetSatEncodingVariableMapResponse>(
      'GET',
      `/problems/${id}/sat-encoding/variable-map`,
    );
  }

  createProblem(request: CreateProblemRequest): Promise<CreateProblemResponse> {
    return this.request<CreateProblemResponse>('POST', '/problems', request);
  }

  updateProblem(id: string, request: UpdateProblemRequest): Promise<void> {
    return this.request<void>('PATCH', `/problems/${id}`, request);
  }

  reduceToSat(id: string, request: ReduceToSatRequest): Promise<void> {
    return this.request<void>('POST', `/problems/${id}/solution`, request);
  }

  // ── Vertex Coloring ────────────────────────────────────────────────────────

  getVertexColoringInstance(id: string): Promise<VertexColoringInstanceDto> {
    return this.request<VertexColoringInstanceDto>(
      'GET',
      `/problems/${id}/instance/vertex-coloring`,
    );
  }

  getVertexColoringSolution(
    id: string,
  ): Promise<GetVertexColoringSolutionResponse> {
    return this.request<GetVertexColoringSolutionResponse>(
      'GET',
      `/problems/${id}/solution/vertex-coloring`,
    );
  }

  setVertexColoringInstance(
    id: string,
    request: SetVertexColoringProblemInstanceRequest,
  ): Promise<void> {
    return this.request<void>(
      'POST',
      `/problems/${id}/instance/vertex-coloring`,
      request,
    );
  }

  // ── Edge Coloring ──────────────────────────────────────────────────────────

  getEdgeColoringInstance(id: string): Promise<EdgeColoringInstanceDto> {
    return this.request<EdgeColoringInstanceDto>(
      'GET',
      `/problems/${id}/instance/edge-coloring`,
    );
  }

  getEdgeColoringSolution(
    id: string,
  ): Promise<GetEdgeColoringSolutionResponse> {
    return this.request<GetEdgeColoringSolutionResponse>(
      'GET',
      `/problems/${id}/solution/edge-coloring`,
    );
  }

  setEdgeColoringInstance(
    id: string,
    request: SetEdgeColoringProblemInstanceRequest,
  ): Promise<void> {
    return this.request<void>(
      'POST',
      `/problems/${id}/instance/edge-coloring`,
      request,
    );
  }

  // ── CSP ────────────────────────────────────────────────────────────────────

  getCspInstance(id: string): Promise<CspInstanceDto> {
    return this.request<CspInstanceDto>('GET', `/problems/${id}/instance/csp`);
  }

  getCspSolution(id: string): Promise<GetCspSolutionResponse> {
    return this.request<GetCspSolutionResponse>(
      'GET',
      `/problems/${id}/solution/csp`,
    );
  }

  setCspInstance(
    id: string,
    request: SetCspProblemInstanceRequest,
  ): Promise<void> {
    return this.request<void>(
      'POST',
      `/problems/${id}/instance/csp`,
      request,
    );
  }

  // ── Boolean ────────────────────────────────────────────────────────────────

  getBooleanInstance(id: string): Promise<BooleanProblemInstanceDto> {
    return this.request<BooleanProblemInstanceDto>(
      'GET',
      `/problems/${id}/instance/bool`,
    );
  }

  getBooleanSolution(id: string): Promise<GetBooleanSolutionResponse> {
    return this.request<GetBooleanSolutionResponse>(
      'GET',
      `/problems/${id}/solution/bool`,
    );
  }

  setBooleanInstance(
    id: string,
    request: SetBooleanProblemInstanceRequest,
  ): Promise<void> {
    return this.request<void>(
      'POST',
      `/problems/${id}/instance/bool`,
      request,
    );
  }

  // ── Boolean Satisfiability (SAT) ───────────────────────────────────────────

  getSatInstance(id: string): Promise<GetSatInstanceResponse> {
    return this.request<GetSatInstanceResponse>(
      'GET',
      `/problems/${id}/instance/sat`,
    );
  }

  getSatSolution(id: string): Promise<GetBooleanSatisfiabilitySolutionResponse> {
    return this.request<GetBooleanSatisfiabilitySolutionResponse>(
      'GET',
      `/problems/${id}/solution/sat`,
    );
  }

  setSatInstance(
    id: string,
    request: SetBooleanSatisfiabilityProblemInstanceRequest,
  ): Promise<void> {
    return this.request<void>(
      'POST',
      `/problems/${id}/instance/sat`,
      request,
    );
  }
}
