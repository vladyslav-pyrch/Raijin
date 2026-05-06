import {useEffect, useState} from 'react';
import {api} from '../lib/api';
import type {
  BooleanProblemInstanceDto,
  CspInstanceDto,
  EdgeColoringInstanceDto,
  SatInstanceDto,
  VertexColoringInstanceDto,
} from '../services/combinatorics';

export type AnyInstanceData =
    | BooleanProblemInstanceDto
    | SatInstanceDto
    | CspInstanceDto
    | VertexColoringInstanceDto
    | EdgeColoringInstanceDto;

interface UseInstanceResult {
    instance: AnyInstanceData | null;
    loading: boolean;
    error: string | null;
}

export function useInstance(
    id: string,
    instanceType: string | null,
): UseInstanceResult {
    const [instance, setInstance] = useState<AnyInstanceData | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!instanceType) return;

        let cancelled = false;
        setLoading(true);
        setError(null);
        setInstance(null);

        // Instance type values come from backend ProblemTypes: "bool" | "sat" | "csp" | "vertex-coloring" | "edge-coloring"
        const promise = (() => {
            switch (instanceType) {
                case 'bool':
                    return api.getBooleanInstance(id);
                case 'sat':
                    return api.getSatInstance(id);
                case 'csp':
                    return api.getCspInstance(id);
                case 'vertex-coloring':
                    return api.getVertexColoringInstance(id);
                case 'edge-coloring':
                    return api.getEdgeColoringInstance(id);
                default:
                    return Promise.reject(new Error(`Unknown instance type: ${instanceType}`));
            }
        })();

        promise
            .then((res) => {
                if (!cancelled) setInstance(res);
            })
            .catch((err: unknown) => {
                if (!cancelled)
                    setError(err instanceof Error ? err.message : 'Failed to load instance');
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });

        return () => {
            cancelled = true;
        };
    }, [id, instanceType]);

    return {instance, loading, error};
}
