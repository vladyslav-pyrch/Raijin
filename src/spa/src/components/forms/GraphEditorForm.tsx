import {useCallback, useEffect, useRef, useState} from 'react';
import type {EdgeDto, GraphDto, VertexDto} from '../../services/combinatorics';

// ─── Types ────────────────────────────────────────────────────────────────────

interface GraphVertex {
    id: string;   // internal stable key
    name: string; // display name + edge reference (can be edited)
    x: number;
    y: number;
}

interface GraphEdge {
    label: string;
    u: string; // vertex name
    v: string; // vertex name
}

interface ViewBox {
    x: number;
    y: number;
    w: number;
    h: number;
}

type Mode = 'add' | 'move';
type GraphInputMode = 'manual' | 'file';

// ─── Constants ────────────────────────────────────────────────────────────────

const SVG_W = 800;
const SVG_H = 420;
const VERTEX_R = 22;
const MIN_VB_W = SVG_W * 0.15;
const MAX_VB_W = SVG_W * 6;

const INIT_VB: ViewBox = {x: 0, y: 0, w: SVG_W, h: SVG_H};

// ─── Helpers ─────────────────────────────────────────────────────────────────

let _uidCounter = 0;

function uid() {
    return `_v${++_uidCounter}`;
}

/**
 * Restore GraphVertex[] from VertexDto[] that already carry stored coordinates.
 * Use this when prefilling the editor from a saved graph instance.
 */
export function layoutFromVertexDtos(dtos: VertexDto[]): GraphVertex[] {
    return dtos.map((v) => ({
        id: uid(),
        name: v.id,
        x: v.x,
        y: v.y,
    }));
}

/** Lay out vertices in a circle for pre-fill when no positions are known. */
export function circleLayout(ids: string[]): GraphVertex[] {
    const cx = SVG_W / 2;
    const cy = SVG_H / 2;
    const n = ids.length;
    const r = n <= 1 ? 0 : Math.min(SVG_W, SVG_H) * 0.36;
    return ids.map((id, i) => ({
        id: uid(),
        name: id,
        x: cx + r * Math.cos((2 * Math.PI * i) / (n || 1) - Math.PI / 2),
        y: cy + r * Math.sin((2 * Math.PI * i) / (n || 1) - Math.PI / 2),
    }));
}

/** Convert screen client coords to SVG data coords using the SVG's current CTM. */
function clientToSvg(svg: SVGSVGElement, clientX: number, clientY: number): { x: number; y: number } {
    const pt = svg.createSVGPoint();
    pt.x = clientX;
    pt.y = clientY;
    const ctm = svg.getScreenCTM();
    if (!ctm) return {x: 0, y: 0};
    const p = pt.matrixTransform(ctm.inverse());
    return {x: p.x, y: p.y};
}

// ─── Component ────────────────────────────────────────────────────────────────

interface GraphEditorFormProps {
    onSubmit: (instance: { graph: GraphDto; colorCount: number }) => Promise<void>;
    onDimacsSubmit?: (instance: { file: File; colorCount: number }) => Promise<void>;
    loading: boolean;
    initialVertices?: GraphVertex[];
    initialEdges?: GraphEdge[];
    initialColorCount?: number;
}

export function GraphEditorForm({
                                    onSubmit,
                                    onDimacsSubmit,
                                    loading,
                                    initialVertices,
                                    initialEdges,
                                    initialColorCount,
                                }: GraphEditorFormProps) {
    const [vertices, setVertices] = useState<GraphVertex[]>(initialVertices ?? []);
    const [edges, setEdges] = useState<GraphEdge[]>(initialEdges ?? []);
    const [colorCount, setColorCount] = useState(initialColorCount ?? 3);
    const [inputMode, setInputMode] = useState<GraphInputMode>('manual');
    const [file, setFile] = useState<File | null>(null);
    const [mode, setMode] = useState<Mode>('add');
    const [error, setError] = useState<string | null>(null);

    // Auto-name counters
    const vertexCounter = useRef(initialVertices?.length ?? 0);
    const edgeCounter = useRef(initialEdges?.length ?? 0);

    // Drag/pan state (all in refs — no re-render needed)
    const dragSource = useRef<string | null>(null);   // vertex id being dragged (edge draw)
    const movingVertex = useRef<string | null>(null);  // vertex id being repositioned
    const dragDidMove = useRef(false);                 // true if any significant movement happened
    const panStart = useRef<{
        clientX: number; clientY: number;
        vbX: number; vbY: number;
        scaleX: number; scaleY: number;
    } | null>(null);

    const [ghostLine, setGhostLine] = useState<{ x1: number; y1: number; x2: number; y2: number } | null>(null);

    // Inline editing state
    const [editingVertex, setEditingVertex] = useState<string | null>(null);
    const [editingEdge, setEditingEdge] = useState<string | null>(null);
    const [editLabel, setEditLabel] = useState('');
    const editInputRef = useRef<HTMLInputElement | null>(null);

    // ViewBox + synced ref
    const [viewBox, setViewBox] = useState<ViewBox>(INIT_VB);
    const viewBoxRef = useRef<ViewBox>(INIT_VB);
    viewBoxRef.current = viewBox;

    const svgRef = useRef<SVGSVGElement>(null);

    // Vertices ref for access inside native handlers
    const verticesRef = useRef(vertices);
    verticesRef.current = vertices;

    // ── Focus edit input ──────────────────────────────────────────────────────

    useEffect(() => {
        if ((editingVertex || editingEdge) && editInputRef.current) {
            editInputRef.current.focus();
            editInputRef.current.select();
        }
    }, [editingVertex, editingEdge]);

    // ── Native wheel handler ──────────────────────────────────────────────────

    useEffect(() => {
        const svg = svgRef.current;
        if (!svg) return;
        const onWheel = (e: WheelEvent) => {
            e.preventDefault();
            const factor = e.deltaY > 0 ? 1.15 : 0.87;
            const {x: cx, y: cy} = clientToSvg(svg, e.clientX, e.clientY);
            setViewBox((vb) => {
                const newW = Math.min(MAX_VB_W, Math.max(MIN_VB_W, vb.w * factor));
                const sf = newW / vb.w;
                return {
                    x: cx - (cx - vb.x) * sf,
                    y: cy - (cy - vb.y) * sf,
                    w: newW,
                    h: (newW / SVG_W) * SVG_H,
                };
            });
        };
        svg.addEventListener('wheel', onWheel, {passive: false});
        return () => svg.removeEventListener('wheel', onWheel);
    }, []);

    // ── Zoom buttons ──────────────────────────────────────────────────────────

    const applyZoom = useCallback((factor: number) => {
        setViewBox((vb) => {
            const cx = vb.x + vb.w / 2;
            const cy = vb.y + vb.h / 2;
            const newW = Math.min(MAX_VB_W, Math.max(MIN_VB_W, vb.w * factor));
            const sf = newW / vb.w;
            return {x: cx - (cx - vb.x) * sf, y: cy - (cy - vb.y) * sf, w: newW, h: (newW / SVG_W) * SVG_H};
        });
    }, []);

    const handleZoomIn = () => applyZoom(0.87);
    const handleZoomOut = () => applyZoom(1.15);
    const handleResetZoom = () => setViewBox(INIT_VB);

    // ── Coordinate helper ─────────────────────────────────────────────────────

    const getSvgCoords = useCallback((clientX: number, clientY: number) => {
        const svg = svgRef.current;
        if (!svg) return {x: 0, y: 0};
        return clientToSvg(svg, clientX, clientY);
    }, []);

    // ── Inline editing ────────────────────────────────────────────────────────

    const startEditVertex = (e: React.MouseEvent, id: string) => {
        e.stopPropagation();
        const v = vertices.find((vx) => vx.id === id);
        if (!v) return;
        setEditingVertex(id);
        setEditingEdge(null);
        setEditLabel(v.name);
    };

    const startEditEdge = (e: React.MouseEvent, label: string) => {
        e.stopPropagation();
        setEditingEdge(label);
        setEditingVertex(null);
        setEditLabel(label);
    };

    const commitVertexEdit = () => {
        if (!editingVertex) return;
        const trimmed = editLabel.trim();
        if (trimmed && trimmed !== vertices.find((v) => v.id === editingVertex)?.name) {
            if (vertices.some((v) => v.id !== editingVertex && v.name === trimmed)) {
                setError(`Vertex name "${trimmed}" already exists`);
                setEditingVertex(null);
                return;
            }
            const oldName = vertices.find((v) => v.id === editingVertex)?.name ?? '';
            setVertices((prev) =>
                prev.map((v) => (v.id === editingVertex ? {...v, name: trimmed} : v)),
            );
            setEdges((prev) =>
                prev.map((ed) => ({
                    ...ed,
                    u: ed.u === oldName ? trimmed : ed.u,
                    v: ed.v === oldName ? trimmed : ed.v,
                })),
            );
        }
        setEditingVertex(null);
        setEditLabel('');
    };

    const commitEdgeEdit = () => {
        if (!editingEdge) return;
        const trimmed = editLabel.trim();
        if (trimmed && trimmed !== editingEdge) {
            if (edges.some((ed) => ed.label !== editingEdge && ed.label === trimmed)) {
                setError(`Edge label "${trimmed}" already exists`);
                setEditingEdge(null);
                return;
            }
            setEdges((prev) =>
                prev.map((ed) => (ed.label === editingEdge ? {...ed, label: trimmed} : ed)),
            );
        }
        setEditingEdge(null);
        setEditLabel('');
    };

    const cancelEdit = () => {
        setEditingVertex(null);
        setEditingEdge(null);
        setEditLabel('');
    };

    // ── SVG background mouse events ───────────────────────────────────────────

    const handleSvgMouseDown = (e: React.MouseEvent<SVGSVGElement>) => {
        const tag = (e.target as Element).tagName;
        if (tag !== 'svg' && tag !== 'rect') return;
        if (editingVertex || editingEdge) return;

        dragDidMove.current = false;

        const svg = svgRef.current;
        if (!svg) return;
        const rect = svg.getBoundingClientRect();
        const vb = viewBoxRef.current;
        panStart.current = {
            clientX: e.clientX,
            clientY: e.clientY,
            vbX: vb.x,
            vbY: vb.y,
            scaleX: vb.w / rect.width,
            scaleY: vb.h / rect.height,
        };
    };

    const handleSvgClick = (e: React.MouseEvent<SVGSVGElement>) => {
        if (mode !== 'add') return;
        if (editingVertex || editingEdge) return;

        const tag = (e.target as Element).tagName;
        if (tag !== 'svg' && tag !== 'rect') return;
        if (dragDidMove.current) return;

        const {x, y} = getSvgCoords(e.clientX, e.clientY);
        const name = `v${++vertexCounter.current}`;
        setVertices((prev) => [...prev, {id: uid(), name, x, y}]);
    };

    // ── Vertex mouse events ───────────────────────────────────────────────────

    const handleVertexMouseDown = (e: React.MouseEvent, vertexId: string) => {
        if (editingVertex || editingEdge) return;
        e.stopPropagation();
        e.preventDefault();
        dragDidMove.current = false;
        panStart.current = null;
        if (mode === 'add') {
            dragSource.current = vertexId;
        } else {
            movingVertex.current = vertexId;
        }
    };

    const handleSvgMouseMove = (e: React.MouseEvent<SVGSVGElement>) => {
        // Pan
        if (panStart.current && !dragSource.current && !movingVertex.current) {
            dragDidMove.current = true;
            const {clientX: cx0, clientY: cy0, vbX, vbY, scaleX, scaleY} = panStart.current;
            setViewBox((vb) => ({
                ...vb,
                x: vbX - (e.clientX - cx0) * scaleX,
                y: vbY - (e.clientY - cy0) * scaleY,
            }));
            return;
        }

        // Edge-draw ghost line
        if (mode === 'add' && dragSource.current) {
            dragDidMove.current = true;
            const src = verticesRef.current.find((v) => v.id === dragSource.current);
            if (!src) return;
            const {x, y} = getSvgCoords(e.clientX, e.clientY);
            setGhostLine({x1: src.x, y1: src.y, x2: x, y2: y});
            return;
        }

        // Move vertex
        if (mode === 'move' && movingVertex.current) {
            dragDidMove.current = true;
            const {x, y} = getSvgCoords(e.clientX, e.clientY);
            setVertices((prev) =>
                prev.map((v) => (v.id === movingVertex.current ? {...v, x, y} : v)),
            );
        }
    };

    const stopDrag = useCallback(() => {
        dragSource.current = null;
        movingVertex.current = null;
        panStart.current = null;
        // dragDidMove intentionally NOT reset here — click fires AFTER mouseup,
        // so the flag must still be true when handleSvgClick / handleVertexClick runs.
        // It resets at the START of the next mousedown instead.
        setGhostLine(null);
    }, []);

    const handleVertexMouseUp = (e: React.MouseEvent, targetId: string) => {
        e.stopPropagation();
        if (mode === 'add' && dragSource.current && dragSource.current !== targetId) {
            const srcId = dragSource.current;
            const srcV = verticesRef.current.find((v) => v.id === srcId);
            const tgtV = verticesRef.current.find((v) => v.id === targetId);
            dragSource.current = null;
            setGhostLine(null);
            if (srcV && tgtV) {
                const label = `e${++edgeCounter.current}`;
                setEdges((prev) => [...prev, {label, u: srcV.name, v: tgtV.name}]);
            }
        } else {
            stopDrag();
        }
    };

    const handleVertexClick = (e: React.MouseEvent, vertexId: string) => {
        if (mode !== 'add') return;
        if (dragDidMove.current) return;
        e.stopPropagation();
        startEditVertex(e, vertexId);
    };

    // ── Context menus ─────────────────────────────────────────────────────────

    const handleVertexContextMenu = (e: React.MouseEvent, vertexId: string) => {
        e.preventDefault();
        const v = vertices.find((vx) => vx.id === vertexId);
        if (!v) return;
        setVertices((prev) => prev.filter((vx) => vx.id !== vertexId));
        setEdges((prev) => prev.filter((ed) => ed.u !== v.name && ed.v !== v.name));
    };

    const handleEdgeContextMenu = (e: React.MouseEvent, label: string) => {
        e.preventDefault();
        setEdges((prev) => prev.filter((ed) => ed.label !== label));
    };

    // ── Submit ────────────────────────────────────────────────────────────────

    const handleSubmit = async () => {
        setError(null);
        if (inputMode === 'file') {
            if (!onDimacsSubmit) {
                setError('File upload is not available here');
                return;
            }
            if (colorCount < 1) {
                setError('Color count must be at least 1');
                return;
            }
            if (!file) {
                setError('Choose a .col file');
                return;
            }
            if (!file.name.toLowerCase().endsWith('.col')) {
                setError('Graph DIMACS file must use .col extension');
                return;
            }

            await onDimacsSubmit({file, colorCount});
            return;
        }

        if (vertices.length === 0) {
            setError('Add at least one vertex');
            return;
        }
        if (colorCount < 1) {
            setError('Color count must be at least 1');
            return;
        }
        const graphVertices: VertexDto[] = vertices.map((v) => ({id: v.name, x: Math.round(v.x), y: Math.round(v.y)}));
        const graphEdges: EdgeDto[] = edges;
        await onSubmit({graph: {vertices: graphVertices, edges: graphEdges}, colorCount});
    };

    // ── Render ────────────────────────────────────────────────────────────────

    const vbStr = `${viewBox.x} ${viewBox.y} ${viewBox.w} ${viewBox.h}`;

    return (
        <div className="space-y-3">
            {onDimacsSubmit && (
                <div className="inline-flex rounded-md border border-neutral-200 dark:border-neutral-700 bg-white dark:bg-surface-secondary p-1">
                    {(['manual', 'file'] as GraphInputMode[]).map((mode) => (
                        <button
                            key={mode}
                            type="button"
                            onClick={() => {
                                setInputMode(mode);
                                setError(null);
                            }}
                            className={`btn btn-sm capitalize ${inputMode === mode ? 'btn-primary' : 'btn-ghost'}`}
                            disabled={loading}
                        >
                            {mode === 'manual' ? 'Manual' : 'File'}
                        </button>
                    ))}
                </div>
            )}

            {inputMode === 'manual' ? (
                <>
            {/* Toolbar */}
            <div className="flex items-center gap-3 flex-wrap">
                <span className="text-xs font-medium text-neutral-500 dark:text-neutral-400">Mode:</span>
                {(['add', 'move'] as Mode[]).map((m) => (
                    <button
                        key={m}
                        onClick={() => setMode(m)}
                        className={`btn btn-sm capitalize ${mode === m ? 'btn-primary' : 'btn-secondary'}`}
                    >
                        {m}
                    </button>
                ))}
                <span className="text-xs text-neutral-400 dark:text-neutral-500">
          {mode === 'add'
              ? 'Click canvas → add vertex. Drag vertex→vertex → edge. Click label → rename. Right-click → delete. Drag background → pan.'
              : 'Drag vertex → reposition. Drag background → pan.'}
        </span>
            </div>

            {/* Canvas wrapper */}
            <div
                className="relative w-full rounded border"
                style={{borderColor: 'var(--canvas-border)', userSelect: 'none'}}
            >
                {/* Zoom controls */}
                <div className="absolute right-2 top-2 z-10 flex flex-col gap-1">
                    {[
                        {label: '+', title: 'Zoom in', fn: handleZoomIn},
                        {label: '−', title: 'Zoom out', fn: handleZoomOut},
                        {label: '⤢', title: 'Reset zoom', fn: handleResetZoom},
                    ].map(({label, title, fn}) => (
                        <button
                            key={label}
                            onClick={fn}
                            title={title}
                            className="flex h-7 w-7 cursor-pointer items-center justify-center rounded text-sm font-bold"
                            style={{
                                background: 'var(--canvas-zoom-bg)',
                                border: '1px solid var(--canvas-zoom-border)',
                                color: 'var(--canvas-zoom-text)',
                            }}
                        >
                            {label}
                        </button>
                    ))}
                </div>

                <svg
                    ref={svgRef}
                    viewBox={vbStr}
                    className="w-full"
                    style={{
                        height: 420,
                        background: 'var(--canvas-bg)',
                        display: 'block',
                        cursor: panStart.current ? 'grabbing' : mode === 'move' ? 'grab' : 'crosshair',
                        userSelect: 'none',
                    }}
                    onMouseDown={handleSvgMouseDown}
                    onClick={handleSvgClick}
                    onMouseMove={handleSvgMouseMove}
                    onMouseUp={stopDrag}
                    onMouseLeave={stopDrag}
                >
                    <rect width={SVG_W} height={SVG_H} fill="transparent"/>

                    {/* Edges */}
                    {edges.map((edge) => {
                        const u = vertices.find((v) => v.name === edge.u);
                        const v = vertices.find((v) => v.name === edge.v);
                        if (!u || !v) return null;
                        const mx = (u.x + v.x) / 2;
                        const my = (u.y + v.y) / 2;
                        return (
                            <g key={edge.label}>
                                <line x1={u.x} y1={u.y} x2={v.x} y2={v.y} stroke="var(--canvas-edge-stroke)"
                                      strokeWidth={2}/>
                                {/* Wide transparent hit area for right-click */}
                                <line
                                    x1={u.x} y1={u.y} x2={v.x} y2={v.y}
                                    stroke="transparent" strokeWidth={16}
                                    onContextMenu={(e) => handleEdgeContextMenu(e, edge.label)}
                                    style={{cursor: 'context-menu'}}
                                />
                                {editingEdge === edge.label ? (
                                    <foreignObject x={mx - 36} y={my - 11} width={72} height={22}>
                                        <input
                                            ref={editInputRef}
                                            value={editLabel}
                                            onChange={(e) => setEditLabel(e.target.value)}
                                            onBlur={commitEdgeEdit}
                                            onKeyDown={(e) => {
                                                if (e.key === 'Enter') commitEdgeEdit();
                                                if (e.key === 'Escape') cancelEdit();
                                            }}
                                            className="w-full h-full rounded text-center"
                                            style={{
                                                fontSize: 10,
                                                border: '1.5px solid var(--canvas-edit-border)',
                                                outline: 'none',
                                                background: 'var(--canvas-vertex-fill)',
                                                color: 'var(--canvas-vertex-text)',
                                            }}
                                        />
                                    </foreignObject>
                                ) : (
                                    <text
                                        x={mx} y={my - 7}
                                        textAnchor="middle" fontSize={10}
                                        fill={mode === 'add' ? 'var(--canvas-link-text)' : 'var(--canvas-edge-text)'}
                                        style={{cursor: mode === 'add' ? 'text' : 'default'}}
                                        onClick={(e) => mode === 'add' && startEditEdge(e, edge.label)}
                                    >
                                        {edge.label}
                                    </text>
                                )}
                            </g>
                        );
                    })}

                    {/* Ghost drag line */}
                    {ghostLine && (
                        <line
                            x1={ghostLine.x1} y1={ghostLine.y1}
                            x2={ghostLine.x2} y2={ghostLine.y2}
                            stroke="var(--canvas-ghost-stroke)" strokeWidth={1.5} strokeDasharray="6 3"
                        />
                    )}

                    {/* Vertices */}
                    {vertices.map((v) => (
                        <g
                            key={v.id}
                            transform={`translate(${v.x},${v.y})`}
                            onMouseDown={(e) => handleVertexMouseDown(e, v.id)}
                            onMouseUp={(e) => handleVertexMouseUp(e, v.id)}
                            onClick={(e) => handleVertexClick(e, v.id)}
                            onContextMenu={(e) => handleVertexContextMenu(e, v.id)}
                            style={{cursor: mode === 'move' ? 'grab' : 'pointer'}}
                        >
                            <circle
                                r={VERTEX_R}
                                fill="var(--canvas-vertex-fill)"
                                stroke={editingVertex === v.id ? 'var(--canvas-edit-border)' : 'var(--canvas-vertex-stroke)'}
                                strokeWidth={editingVertex === v.id ? 2.5 : 2}
                            />
                            {editingVertex === v.id ? (
                                <foreignObject x={-VERTEX_R} y={-11} width={VERTEX_R * 2} height={22}>
                                    <input
                                        ref={editInputRef}
                                        value={editLabel}
                                        onChange={(e) => setEditLabel(e.target.value)}
                                        onBlur={commitVertexEdit}
                                        onKeyDown={(e) => {
                                            if (e.key === 'Enter') commitVertexEdit();
                                            if (e.key === 'Escape') cancelEdit();
                                        }}
                                        className="h-full w-full border-0 text-center"
                                        style={{
                                            fontSize: 11,
                                            outline: 'none',
                                            background: 'transparent',
                                            color: 'var(--canvas-vertex-text)',
                                        }}
                                    />
                                </foreignObject>
                            ) : (
                                <text
                                    textAnchor="middle" dominantBaseline="middle"
                                    fontSize={11} fill="var(--canvas-vertex-text)"
                                    style={{userSelect: 'none', pointerEvents: 'none'}}
                                >
                                    {v.name.length > 5 ? v.name.slice(0, 4) + '…' : v.name}
                                </text>
                            )}
                        </g>
                    ))}
                </svg>
            </div>

            {/* Color count + stats */}
            <div className="flex items-center gap-6 flex-wrap">
                <div className="flex items-center gap-2">
                    <label className="text-xs font-medium text-neutral-500 dark:text-neutral-400">Color count:</label>
                    <input
                        type="number" min={1} value={colorCount}
                        onChange={(e) => setColorCount(Number(e.target.value))}
                        className="input w-20"
                        disabled={loading}
                    />
                </div>
                <span className="text-xs text-neutral-400 dark:text-neutral-500">
          {vertices.length} vertices · {edges.length} edges
        </span>
            </div>
                </>
            ) : (
                <div className="space-y-4">
                    <div>
                        <label className="label">DIMACS graph file</label>
                        <input
                            type="file"
                            accept=".col"
                            className="input w-full cursor-pointer"
                            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
                            disabled={loading}
                        />
                        <p className="text-neutral-400 dark:text-neutral-500 text-xs mt-1">
                            Upload a DIMACS graph file with <code
                            className="font-geist-mono bg-neutral-100 dark:bg-surface-tertiary px-1 rounded">.col</code> extension.
                        </p>
                    </div>

                    <div className="flex items-center gap-2">
                        <label className="text-xs font-medium text-neutral-500 dark:text-neutral-400">Color count:</label>
                        <input
                            type="number" min={1} value={colorCount}
                            onChange={(e) => setColorCount(Number(e.target.value))}
                            className="input w-20"
                            disabled={loading}
                        />
                    </div>
                </div>
            )}

            {error && <p className="text-error-500 text-xs">{error}</p>}

            <div className="flex justify-end">
                <button
                    onClick={handleSubmit} disabled={loading}
                    className="btn btn-primary disabled:opacity-50"
                >
                    {loading ? 'Saving…' : inputMode === 'file' ? 'Upload File' : 'Create'}
                </button>
            </div>
        </div>
    );
}
