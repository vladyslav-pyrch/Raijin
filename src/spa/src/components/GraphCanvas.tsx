import {useCallback, useEffect, useMemo, useRef, useState} from 'react';

const BASE_W = 800;
const BASE_H = 400;
const VERTEX_R = 22;

export interface CanvasVertex {
    id: string;
}

export interface CanvasEdge {
    label: string;
    u: string;
    v: string;
}

interface ViewBox {
    x: number;
    y: number;
    w: number;
    h: number;
}

interface CanvasMetrics {
    width: number;
    height: number;
    radius: number;
}

function metricsForVertexCount(vertexCount: number): CanvasMetrics {
    if (vertexCount <= 1) return {width: BASE_W, height: BASE_H, radius: 0};

    const desiredSpacing = 80;
    const radiusForSpacing = (vertexCount * desiredSpacing) / (2 * Math.PI);
    const radius = Math.max(144, radiusForSpacing);
    const diameterWithPadding = radius * 2 + 120;

    return {
        width: Math.max(BASE_W, Math.ceil(diameterWithPadding)),
        height: Math.max(BASE_H, Math.ceil(diameterWithPadding)),
        radius,
    };
}

function buildInitialViewBox(metrics: CanvasMetrics): ViewBox {
    return {x: 0, y: 0, w: metrics.width, h: metrics.height};
}

export function circleLayoutPositions(
    ids: string[],
    metrics: CanvasMetrics = metricsForVertexCount(ids.length),
): Record<string, { x: number; y: number }> {
    const cx = metrics.width / 2;
    const cy = metrics.height / 2;
    const result: Record<string, { x: number; y: number }> = {};
    ids.forEach((id, i) => {
        result[id] = {
            x: cx + metrics.radius * Math.cos((2 * Math.PI * i) / ids.length - Math.PI / 2),
            y: cy + metrics.radius * Math.sin((2 * Math.PI * i) / ids.length - Math.PI / 2),
        };
    });
    return result;
}

function clientToSvg(svg: SVGSVGElement, clientX: number, clientY: number): { x: number; y: number } {
    const pt = svg.createSVGPoint();
    pt.x = clientX;
    pt.y = clientY;
    const ctm = svg.getScreenCTM();
    if (!ctm) return {x: 0, y: 0};
    const p = pt.matrixTransform(ctm.inverse());
    return {x: p.x, y: p.y};
}

interface GraphCanvasProps {
    vertices: CanvasVertex[];
    edges: CanvasEdge[];
    vertexColors?: Record<string, string>;
    edgeColors?: Record<string, string>;
    movable?: boolean;
    height?: number;
}

export function GraphCanvas({
                                vertices,
                                edges,
                                vertexColors,
                                edgeColors,
                                movable = true,
                                height = 300,
                            }: GraphCanvasProps) {
    const svgRef = useRef<SVGSVGElement>(null);
    const metrics = useMemo(() => metricsForVertexCount(vertices.length), [vertices.length]);
    const initialViewBox = useMemo(() => buildInitialViewBox(metrics), [metrics]);

    const vertexKey = vertices.map((v) => v.id).join(',');
    const [positions, setPositions] = useState<Record<string, { x: number; y: number }>>(() =>
        circleLayoutPositions(vertices.map((v) => v.id), metrics),
    );
    useEffect(() => {
        setPositions(circleLayoutPositions(vertices.map((v) => v.id), metrics));
        // vertices are intentionally represented by vertexKey so color-only rerenders do not reset layout
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [vertexKey, metrics]);

    const [viewBox, setViewBox] = useState<ViewBox>(initialViewBox);
    useEffect(() => {
        setViewBox(initialViewBox);
    }, [initialViewBox]);

    const minViewBoxWidth = metrics.width * 0.15;
    const maxViewBoxWidth = metrics.width * 6;
    const viewBoxRef = useRef<ViewBox>(viewBox);
    viewBoxRef.current = viewBox;

    const positionsRef = useRef(positions);
    positionsRef.current = positions;

    const dragging = useRef<string | null>(null);
    const dragOffset = useRef({dx: 0, dy: 0});
    const panStart = useRef<{
        clientX: number; clientY: number;
        vbX: number; vbY: number;
        scaleX: number; scaleY: number;
    } | null>(null);

    const getSvgCoords = useCallback((clientX: number, clientY: number) => {
        const svg = svgRef.current;
        if (!svg) return {x: 0, y: 0};
        return clientToSvg(svg, clientX, clientY);
    }, []);

    useEffect(() => {
        const svg = svgRef.current;
        if (!svg) return;
        const onWheel = (e: WheelEvent) => {
            e.preventDefault();
            const factor = e.deltaY > 0 ? 1.15 : 0.87;
            const {x: cx, y: cy} = clientToSvg(svg, e.clientX, e.clientY);
            setViewBox((vb) => {
                const newW = Math.min(maxViewBoxWidth, Math.max(minViewBoxWidth, vb.w * factor));
                const sf = newW / vb.w;
                return {
                    x: cx - (cx - vb.x) * sf,
                    y: cy - (cy - vb.y) * sf,
                    w: newW,
                    h: (newW / metrics.width) * metrics.height,
                };
            });
        };
        svg.addEventListener('wheel', onWheel, {passive: false});
        return () => svg.removeEventListener('wheel', onWheel);
    }, [maxViewBoxWidth, metrics.height, metrics.width, minViewBoxWidth]);

    const applyZoom = useCallback((factor: number) => {
        setViewBox((vb) => {
            const cx = vb.x + vb.w / 2;
            const cy = vb.y + vb.h / 2;
            const newW = Math.min(maxViewBoxWidth, Math.max(minViewBoxWidth, vb.w * factor));
            const sf = newW / vb.w;
            return {x: cx - (cx - vb.x) * sf, y: cy - (cy - vb.y) * sf, w: newW, h: (newW / metrics.width) * metrics.height};
        });
    }, [maxViewBoxWidth, metrics.height, metrics.width, minViewBoxWidth]);

    const handleVertexMouseDown = (e: React.MouseEvent, id: string) => {
        if (!movable) return;
        e.stopPropagation();
        e.preventDefault();
        dragging.current = id;
        panStart.current = null;

        const svgPt = getSvgCoords(e.clientX, e.clientY);
        const pos = positionsRef.current[id];
        dragOffset.current = pos ? {dx: svgPt.x - pos.x, dy: svgPt.y - pos.y} : {dx: 0, dy: 0};
    };

    const handleSvgMouseDown = (e: React.MouseEvent<SVGSVGElement>) => {
        const tag = (e.target as Element).tagName;
        if (tag !== 'svg' && tag !== 'rect') return;
        e.preventDefault();

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

    const handleMouseMove = useCallback((e: React.MouseEvent) => {
        if (panStart.current && !dragging.current) {
            const p = panStart.current;
            setViewBox((vb) => ({
                ...vb,
                x: p.vbX - (e.clientX - p.clientX) * p.scaleX,
                y: p.vbY - (e.clientY - p.clientY) * p.scaleY,
            }));
            return;
        }

        if (dragging.current) {
            const {x, y} = getSvgCoords(e.clientX, e.clientY);
            const id = dragging.current;
            setPositions((prev) => ({
                ...prev,
                [id]: {x: x - dragOffset.current.dx, y: y - dragOffset.current.dy},
            }));
        }
    }, [getSvgCoords]);

    const handleMouseUp = () => {
        dragging.current = null;
        panStart.current = null;
    };

    return (
        <div className="relative w-full rounded border" style={{borderColor: 'var(--canvas-border)', userSelect: 'none'}}>
            <div className="absolute right-2 top-2 z-10 flex flex-col gap-1">
                {[
                    {label: '+', title: 'Zoom in', onClick: () => applyZoom(0.87)},
                    {label: '−', title: 'Zoom out', onClick: () => applyZoom(1.15)},
                    {label: '⤢', title: 'Reset zoom', onClick: () => setViewBox(initialViewBox)},
                ].map(({label, title, onClick}) => (
                    <button
                        key={label}
                        onClick={onClick}
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
                viewBox={`${viewBox.x} ${viewBox.y} ${viewBox.w} ${viewBox.h}`}
                style={{
                    width: '100%',
                    height,
                    background: 'var(--canvas-bg)',
                    display: 'block',
                    cursor: panStart.current ? 'grabbing' : movable ? 'grab' : 'default',
                    userSelect: 'none',
                }}
                onMouseDown={handleSvgMouseDown}
                onMouseMove={handleMouseMove}
                onMouseUp={handleMouseUp}
                onMouseLeave={handleMouseUp}
            >
                <rect width={metrics.width} height={metrics.height} fill="transparent"/>

                {edges.map((edge) => {
                    const u = positions[edge.u];
                    const v = positions[edge.v];
                    if (!u || !v) return null;
                    const mx = (u.x + v.x) / 2;
                    const my = (u.y + v.y) / 2;
                    const color = edgeColors?.[edge.label];
                    return (
                        <g key={edge.label}>
                            <line x1={u.x} y1={u.y} x2={v.x} y2={v.y} stroke={color ?? 'var(--canvas-edge-stroke)'} strokeWidth={color ? 3 : 2} style={color ? {filter: `drop-shadow(0 0 5px ${color})`} : undefined}/>
                            <text x={mx} y={my - 7} textAnchor="middle" fontSize={10} fill="var(--canvas-edge-text)" style={{userSelect: 'none', pointerEvents: 'none'}}>
                                {edge.label}
                            </text>
                        </g>
                    );
                })}

                {vertices.map((v) => {
                    const pos = positions[v.id];
                    if (!pos) return null;
                    const color = vertexColors?.[v.id];
                    return (
                        <g key={v.id} transform={`translate(${pos.x},${pos.y})`} onMouseDown={(e) => handleVertexMouseDown(e, v.id)} style={{cursor: movable ? 'grab' : 'default'}}>
                            <circle r={VERTEX_R} fill="var(--canvas-vertex-fill)" stroke={color ?? 'var(--canvas-vertex-stroke)'} strokeWidth={color ? 3 : 2} style={color ? {filter: `drop-shadow(0 0 6px ${color})`} : undefined}/>
                            <text textAnchor="middle" dominantBaseline="middle" fontSize={11} fill="var(--canvas-vertex-text)" style={{userSelect: 'none', pointerEvents: 'none'}}>
                                {v.id.length > 5 ? v.id.slice(0, 4) + '…' : v.id}
                            </text>
                        </g>
                    );
                })}
            </svg>

            {vertices.length === 0 && (
                <div className="absolute inset-0 flex items-center justify-center" style={{pointerEvents: 'none'}}>
                    <span className="text-sm" style={{color: 'var(--canvas-hint-text)'}}>No vertices</span>
                </div>
            )}
        </div>
    );
}
