import {useState} from 'react';
import {useLocation, useNavigate} from 'react-router-dom';
import {api} from '../lib/api';
import {INSTANCE_TYPES, type InstanceTypeValue} from '../lib/constants';
import {GraphEditorForm, layoutFromVertexDtos} from '../components/forms/GraphEditorForm';
import {BooleanInstanceForm} from '../components/forms/BooleanInstanceForm';
import {SatInstanceForm} from '../components/forms/SatInstanceForm';
import {CspInstanceForm} from '../components/forms/CspInstanceForm';
import {ErrorStack, useErrorStack} from '../components/ErrorStack';
import type {AnyInstanceData} from '../hooks/useInstance';
import type {
    BooleanProblemInstanceDto,
    CspInstanceDto,
    EdgeColoringInstanceDto,
    SatInstanceDto,
    VertexColoringInstanceDto,
} from '../services/combinatorics';

// ─── Fork prefill (passed via router state) ───────────────────────────────────

export interface ForkPrefill {
    name: string;
    description: string;
    instanceType: InstanceTypeValue;
    instance: AnyInstanceData;
}

export function nextVersionName(name: string): string {
    const match = name.match(/^(.*?)\s*\(version\s+(\d+)\)\s*$/i);
    if (match) {
        const base = (match[1] ?? '').trim();
        const n = parseInt(match[2] ?? '1', 10);
        return `${base} (version ${n + 1})`;
    }
    return `${name} (version 2)`;
}

// ─── Component ────────────────────────────────────────────────────────────────

interface CreateProblemPageProps {
    onProblemChanged: () => void;
}

export function CreateProblemPage({onProblemChanged}: CreateProblemPageProps) {
    const navigate = useNavigate();
    const location = useLocation();
    const prefill = (location.state as { fork?: ForkPrefill } | null)?.fork;
    const isFork = prefill !== undefined;

    const [selectedType, setSelectedType] = useState<InstanceTypeValue | null>(
        prefill?.instanceType ?? null,
    );
    const [name, setName] = useState(isFork ? nextVersionName(prefill!.name) : '');
    const [description, setDescription] = useState(isFork ? (prefill!.description ?? '') : '');
    const [loading, setLoading] = useState(false);

    const {errors, addError, dismiss} = useErrorStack();

    // ── Submit wrapper ────────────────────────────────────────────────────────

    const wrap = async (fn: () => Promise<string>) => {
        if (!name.trim()) {
            addError('Name is required');
            return;
        }
        setLoading(true);
        try {
            const id = await fn();
            await onProblemChanged();
            navigate(`/problems/${id}`);
        } catch (err) {
            addError(err instanceof Error ? err.message : 'Failed to create problem');
        } finally {
            setLoading(false);
        }
    };

    // ── Per-type submit handlers ──────────────────────────────────────────────

    const handleBoolSubmit = async (formula: string) => {
        await wrap(async () => {
            const res = await api.createBooleanProblem({
                name: name.trim(),
                description: description || null,
                instance: {formula},
            });
            return res.problemId;
        });
    };

    const handleSatSubmit = async (clauses: string[][]) => {
        await wrap(async () => {
            const res = await api.createSatProblem({
                name: name.trim(),
                description: description || null,
                instance: {clauses},
            });
            return res.problemId;
        });
    };

    const handleSatDimacsSubmit = async (file: File) => {
        await wrap(async () => {
            const res = await api.createSatProblemFromDimacs({
                name: name.trim(),
                description: description || null,
                file,
            });
            return res.problemId;
        });
    };

    const handleCspSubmit = async (instance: CspInstanceDto) => {
        await wrap(async () => {
            const res = await api.createCspProblem({
                name: name.trim(),
                description: description || null,
                instance,
            });
            return res.problemId;
        });
    };

    const handleVertexColoringSubmit = async (inst: {
        graph: import('../services/combinatorics').GraphDto;
        colorCount: number
    }) => {
        await wrap(async () => {
            const res = await api.createVertexColoringProblem({
                name: name.trim(),
                description: description || null,
                instance: inst,
            });
            return res.problemId;
        });
    };

    const handleVertexColoringDimacsSubmit = async (inst: {
        file: File;
        colorCount: number;
    }) => {
        await wrap(async () => {
            const res = await api.createVertexColoringProblemFromDimacs({
                name: name.trim(),
                description: description || null,
                file: inst.file,
                colorCount: inst.colorCount,
            });
            return res.problemId;
        });
    };

    const handleEdgeColoringSubmit = async (inst: {
        graph: import('../services/combinatorics').GraphDto;
        colorCount: number
    }) => {
        await wrap(async () => {
            const res = await api.createEdgeColoringProblem({
                name: name.trim(),
                description: description || null,
                instance: inst,
            });
            return res.problemId;
        });
    };

    const handleEdgeColoringDimacsSubmit = async (inst: {
        file: File;
        colorCount: number;
    }) => {
        await wrap(async () => {
            const res = await api.createEdgeColoringProblemFromDimacs({
                name: name.trim(),
                description: description || null,
                file: inst.file,
                colorCount: inst.colorCount,
            });
            return res.problemId;
        });
    };

    // ── Build pre-fill props for instance forms ───────────────────────────────

    const getPrefillProps = (): Record<string, unknown> => {
        if (!prefill) return {};
        switch (prefill.instanceType) {
            case 'bool': {
                const d = prefill.instance as BooleanProblemInstanceDto;
                return {initialFormula: d.formula};
            }
            case 'sat': {
                const d = prefill.instance as SatInstanceDto;
                return {initialText: d.clauses.map((c: string[]) => c.join(' ')).join('\n')};
            }
            case 'csp': {
                const d = prefill.instance as CspInstanceDto;
                return {
                    initialVariables: d.variables.map((v) => ({name: v.name, states: v.states.join(', ')})),
                    initialConstraints: d.constraints.length ? d.constraints : [''],
                };
            }
            case 'vertex-coloring':
            case 'edge-coloring': {
                const d = prefill.instance as VertexColoringInstanceDto | EdgeColoringInstanceDto;
                return {
                    initialVertices: layoutFromVertexDtos(d.graph.vertices),
                    initialEdges: d.graph.edges,
                    initialColorCount: d.colorCount,
                };
            }
            default:
                return {};
        }
    };

    const prefillProps = getPrefillProps();

    // ── Instance form for selected type ──────────────────────────────────────

    const renderForm = () => {
        switch (selectedType) {
            case 'bool':
                return (
                    <BooleanInstanceForm
                        loading={loading}
                        onSubmit={handleBoolSubmit}
                        {...(prefillProps as { initialFormula?: string })}
                    />
                );
            case 'sat':
                return (
                    <SatInstanceForm
                        loading={loading}
                        onSubmit={handleSatSubmit}
                        onDimacsSubmit={handleSatDimacsSubmit}
                        {...(prefillProps as { initialText?: string })}
                    />
                );
            case 'csp':
                return (
                    <CspInstanceForm
                        loading={loading}
                        onSubmit={handleCspSubmit}
                        {...(prefillProps as object)}
                    />
                );
            case 'vertex-coloring':
                return (
                    <GraphEditorForm
                        loading={loading}
                        onSubmit={handleVertexColoringSubmit}
                        onDimacsSubmit={handleVertexColoringDimacsSubmit}
                        {...(prefillProps as object)}
                    />
                );
            case 'edge-coloring':
                return (
                    <GraphEditorForm
                        loading={loading}
                        onSubmit={handleEdgeColoringSubmit}
                        onDimacsSubmit={handleEdgeColoringDimacsSubmit}
                        {...(prefillProps as object)}
                    />
                );
            default:
                return null;
        }
    };

    const pageTitle = isFork
        ? 'New version'
        : selectedType
            ? `New problem — ${INSTANCE_TYPES.find((t) => t.value === selectedType)?.label ?? selectedType}`
            : 'New problem';

    // ── Render ────────────────────────────────────────────────────────────────

    return (
        <div className="flex flex-col h-full">
            {/* Page header */}
            <div
                className="shrink-0 border-b border-neutral-200 dark:border-neutral-700 px-6 py-4 bg-white dark:bg-surface-secondary">
                <p className="text-xs mb-1 text-neutral-400 dark:text-neutral-500">
                    Problems <span className="mx-1">›</span>
                    <span className="text-primary-500 dark:text-primary-400">
            {isFork ? `${prefill!.name} › New version` : 'New problem'}
          </span>
                </p>
                <div className="flex items-center justify-between">
                    <h1 className="text-lg font-semibold text-neutral-900 dark:text-neutral-100">
                        {pageTitle}
                    </h1>
                    <button
                        onClick={() => navigate(-1)}
                        className="btn btn-secondary btn-sm"
                    >
                        Cancel
                    </button>
                </div>
            </div>

            {/* Content */}
            <div className="flex-1 overflow-y-auto px-6 py-5">
                <div className="w-full space-y-5">
                    <ErrorStack errors={errors} onDismiss={dismiss}/>

                    {/* Name */}
                    <div>
                        <label className="label">
                            Name <span className="text-error-500">*</span>
                        </label>
                        <input
                            className="input w-full"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            disabled={loading}
                            placeholder="Problem name"
                        />
                    </div>

                    {/* Description */}
                    <div>
                        <label className="label">Description</label>
                        <textarea
                            className="input w-full resize-y"
                            rows={3}
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            disabled={loading}
                            placeholder="Optional description…"
                        />
                    </div>

                    {/* Type picker — create mode only, before type is selected */}
                    {!isFork && !selectedType && (
                        <div>
                            <p className="label">Choose problem type:</p>
                            <div className="space-y-1.5">
                                {INSTANCE_TYPES.map((t) => (
                                    <button
                                        key={t.value}
                                        onClick={() => setSelectedType(t.value)}
                                        className="w-full rounded-md border px-4 py-3 text-left transition-colors cursor-pointer
                               border-neutral-200 dark:border-neutral-700
                               bg-white dark:bg-surface-secondary
                               text-neutral-900 dark:text-neutral-100
                               hover:bg-primary-50 dark:hover:bg-primary-900/20
                               hover:border-primary-300 dark:hover:border-primary-700"
                                    >
                                        <span className="text-sm font-medium">{t.label}</span>
                                    </button>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Back button — create mode, after type selected */}
                    {!isFork && selectedType && (
                        <button
                            onClick={() => setSelectedType(null)}
                            className="link text-sm cursor-pointer"
                        >
                            ← Back to type selection
                        </button>
                    )}

                    {/* Instance form */}
                    {selectedType && renderForm()}
                </div>
            </div>
        </div>
    );
}
