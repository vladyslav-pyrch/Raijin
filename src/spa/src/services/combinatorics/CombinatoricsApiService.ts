import type {
    BooleanProblemInstanceDto,
    CreateBooleanProblemRequest,
    CreateBooleanProblemResponse,
    CreateCspProblemRequest,
    CreateCspProblemResponse,
    CreateEdgeColoringProblemFromDimacsRequest,
    CreateEdgeColoringProblemRequest,
    CreateEdgeColoringProblemResponse,
    CreateSatProblemFromDimacsRequest,
    CreateSatProblemRequest,
    CreateSatProblemResponse,
    CreateVertexColoringProblemFromDimacsRequest,
    CreateVertexColoringProblemRequest,
    CreateVertexColoringProblemResponse,
    CspInstanceDto,
    EdgeColoringInstanceDto,
    GetBooleanInstanceResponse,
    GetBooleanSatisfiabilityInstanceResponse,
    GetBooleanSatisfiabilitySolutionResponse,
    GetBooleanSolutionResponse,
    GetCspInstanceResponse,
    GetCspSolutionResponse,
    GetEdgeColoringInstanceResponse,
    GetEdgeColoringSolutionResponse,
    GetProblemResponse,
    GetSatEncodingResponse,
    GetSatEncodingVariableMapResponse,
    GetVertexColoringInstanceResponse,
    GetVertexColoringSolutionResponse,
    ListProblemsResponse,
    ProblemDetails,
    SatInstanceDto,
    UpdateProblemRequest,
    VertexColoringInstanceDto,
} from './types';

// ─── Error ───────────────────────────────────────────────────────────────────

export class ApiError extends Error {
    readonly status: number;
    readonly details: ProblemDetails;

    constructor(
        status: number,
        details: ProblemDetails,
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
        this.baseUrl = baseUrl + "/api/combinatorics";
    }

    // ── Core helper ────────────────────────────────────────────────────────────

    getProblem(id: string): Promise<GetProblemResponse> {
        return this.request<GetProblemResponse>('GET', `/problems/${id}`);
    }

    // ── Problems ───────────────────────────────────────────────────────────────

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

    updateProblem(id: string, request: UpdateProblemRequest): Promise<void> {
        return this.request<void>('PATCH', `/problems/${id}`, request);
    }

    /** Trigger reduction + solve. solver passed as query param. */
    solve(id: string, solver: string): Promise<void> {
        return this.request<void>('POST', `/problems/${id}/solve?solver=${encodeURIComponent(solver)}`);
    }

    createBooleanProblem(request: CreateBooleanProblemRequest): Promise<CreateBooleanProblemResponse> {
        return this.request<CreateBooleanProblemResponse>('POST', '/problems/bool', request);
    }

    // ── Boolean ────────────────────────────────────────────────────────────────

    async getBooleanInstance(id: string): Promise<BooleanProblemInstanceDto> {
        const res = await this.request<GetBooleanInstanceResponse>('GET', `/problems/${id}/bool/instance`);
        return res.instance;
    }

    getBooleanSolution(id: string): Promise<GetBooleanSolutionResponse> {
        return this.request<GetBooleanSolutionResponse>('GET', `/problems/${id}/bool/solution`);
    }

    createSatProblem(request: CreateSatProblemRequest): Promise<CreateSatProblemResponse> {
        return this.request<CreateSatProblemResponse>('POST', '/problems/sat', request);
    }

    createSatProblemFromDimacs(request: CreateSatProblemFromDimacsRequest): Promise<CreateSatProblemResponse> {
        return this.request<CreateSatProblemResponse>(
            'POST',
            '/problems/sat-dimacs',
            this.createDimacsFormData(request),
        );
    }

    // ── Boolean Satisfiability (SAT) ───────────────────────────────────────────

    async getSatInstance(id: string): Promise<SatInstanceDto> {
        const res = await this.request<GetBooleanSatisfiabilityInstanceResponse>('GET', `/problems/${id}/sat/instance`);
        return res.instance;
    }

    getSatSolution(id: string): Promise<GetBooleanSatisfiabilitySolutionResponse> {
        return this.request<GetBooleanSatisfiabilitySolutionResponse>('GET', `/problems/${id}/sat/solution`);
    }

    createCspProblem(request: CreateCspProblemRequest): Promise<CreateCspProblemResponse> {
        return this.request<CreateCspProblemResponse>('POST', '/problems/csp', request);
    }

    // ── CSP ────────────────────────────────────────────────────────────────────

    async getCspInstance(id: string): Promise<CspInstanceDto> {
        const res = await this.request<GetCspInstanceResponse>('GET', `/problems/${id}/csp/instance`);
        return res.instance;
    }

    getCspSolution(id: string): Promise<GetCspSolutionResponse> {
        return this.request<GetCspSolutionResponse>('GET', `/problems/${id}/csp/solution`);
    }

    createVertexColoringProblem(request: CreateVertexColoringProblemRequest): Promise<CreateVertexColoringProblemResponse> {
        return this.request<CreateVertexColoringProblemResponse>('POST', '/problems/vertex-coloring', request);
    }

    createVertexColoringProblemFromDimacs(request: CreateVertexColoringProblemFromDimacsRequest): Promise<CreateVertexColoringProblemResponse> {
        return this.request<CreateVertexColoringProblemResponse>(
            'POST',
            '/problems/vertex-coloring-dimacs',
            this.createDimacsFormData(request),
        );
    }

    // ── Vertex Coloring ────────────────────────────────────────────────────────

    async getVertexColoringInstance(id: string): Promise<VertexColoringInstanceDto> {
        const res = await this.request<GetVertexColoringInstanceResponse>('GET', `/problems/${id}/vertex-coloring/instance`);
        return res.instance;
    }

    getVertexColoringSolution(id: string): Promise<GetVertexColoringSolutionResponse> {
        return this.request<GetVertexColoringSolutionResponse>('GET', `/problems/${id}/vertex-coloring/solution`);
    }

    createEdgeColoringProblem(request: CreateEdgeColoringProblemRequest): Promise<CreateEdgeColoringProblemResponse> {
        return this.request<CreateEdgeColoringProblemResponse>('POST', '/problems/edge-coloring', request);
    }

    createEdgeColoringProblemFromDimacs(request: CreateEdgeColoringProblemFromDimacsRequest): Promise<CreateEdgeColoringProblemResponse> {
        return this.request<CreateEdgeColoringProblemResponse>(
            'POST',
            '/problems/edge-coloring-dimacs',
            this.createDimacsFormData(request),
        );
    }

    // ── Edge Coloring ──────────────────────────────────────────────────────────

    async getEdgeColoringInstance(id: string): Promise<EdgeColoringInstanceDto> {
        const res = await this.request<GetEdgeColoringInstanceResponse>('GET', `/problems/${id}/edge-coloring/instance`);
        return res.instance;
    }

    getEdgeColoringSolution(id: string): Promise<GetEdgeColoringSolutionResponse> {
        return this.request<GetEdgeColoringSolutionResponse>('GET', `/problems/${id}/edge-coloring/solution`);
    }

    private async request<T = void>(
        method: string,
        path: string,
        body?: unknown,
    ): Promise<T> {
        const headers: Record<string, string> = {
            Accept: 'application/json',
        };

        if (body !== undefined) {
            if (!(body instanceof FormData)) {
                headers['Content-Type'] = 'application/json';
            }
        }

        const url = `${this.baseUrl.replace(/\/$/, '')}${path}`;

        const init: RequestInit = {method: method, headers: headers};
        if (body !== undefined) {
            init.body = body instanceof FormData ? body : JSON.stringify(body);
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
                details = {status: response.status, title: response.statusText};
            }
            throw new ApiError(response.status, details);
        }

        return response.json() as Promise<T>;
    }

    private createDimacsFormData(request: {
        name?: string;
        description?: string | null;
        colorCount?: number;
        file: File;
    }): FormData {
        const formData = new FormData();

        if (request.name !== undefined) {
            formData.append('Name', request.name);
        }

        if (request.description !== undefined && request.description !== null) {
            formData.append('Description', request.description);
        }

        if (request.colorCount !== undefined) {
            formData.append('ColorCount', String(request.colorCount));
        }

        formData.append('File', request.file);

        return formData;
    }
}
