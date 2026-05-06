import {useEffect, useState} from 'react';
import {api} from '../lib/api';
import type {
  GetBooleanSatisfiabilitySolutionResponse,
  GetBooleanSolutionResponse,
  GetCspSolutionResponse,
  GetEdgeColoringSolutionResponse,
  GetVertexColoringSolutionResponse,
} from '../services/combinatorics';

type AnySolutionResponse =
    | GetBooleanSolutionResponse
    | GetBooleanSatisfiabilitySolutionResponse
    | GetCspSolutionResponse
    | GetVertexColoringSolutionResponse
    | GetEdgeColoringSolutionResponse;

interface UseSolutionResult {
    solution: AnySolutionResponse | null;
    loading: boolean;
    error: string | null;
}

export function useSolution(
    id: string,
    instanceType: string | null,
    enabled: boolean,
): UseSolutionResult {
    const [solution, setSolution] = useState<AnySolutionResponse | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!enabled || instanceType === null) return;
        let cancelled = false;
        setLoading(true);
        setError(null);

        // Instance type values come from backend ProblemTypes: "bool" | "sat" | "csp" | "vertex-coloring" | "edge-coloring"
        const promise = (() => {
            switch (instanceType) {
                case 'bool':
                    return api.getBooleanSolution(id);
                case 'sat':
                    return api.getSatSolution(id);
                case 'csp':
                    return api.getCspSolution(id);
                case 'vertex-coloring':
                    return api.getVertexColoringSolution(id);
                case 'edge-coloring':
                    return api.getEdgeColoringSolution(id);
                default:
                    return Promise.reject(new Error(`Unknown instance type: ${instanceType}`));
            }
        })();

        promise
            .then((res) => {
                if (!cancelled) setSolution(res);
            })
            .catch((err: unknown) => {
                if (!cancelled)
                    setError(err instanceof Error ? err.message : 'Failed to load solution');
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });

        return () => {
            cancelled = true;
        };
    }, [id, instanceType, enabled]);

    return {solution, loading, error};
}

export type {AnySolutionResponse};
