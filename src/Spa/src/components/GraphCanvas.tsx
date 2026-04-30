import {useCallback, useEffect, useRef, useState} from 'react';

// ─── Constants ────────────────────────────────────────────────────────────────

const SVG_W = 800;
const SVG_H = 400;
const VERTEX_R = 22;
const MIN_VB_W = SVG_W * 0.15;
const MAX_VB_W = SVG_W * 6;

const INIT_VB = { x: 0, y: 0, w: SVG_W, h: SVG_H };

// ─── Types ────────────────────────────────────────────────────────────────────

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

// ─── Layout helper ────────────────────────────────────────────────────────────

export function circleLayoutPositions(ids: string[]): Record<string, { x: number; y: number }> {
  const cx = SVG_W / 2;
  const cy = SVG_H / 2;
  const n = ids.length;
  const r = n <= 1 ? 0 : Math.min(SVG_W, SVG_H) * 0.36;
  const result: Record<string, { x: number; y: number }> = {};
  ids.forEach((id, i) => {
    result[id] = {
      x: cx + r * Math.cos((2 * Math.PI * i) / n - Math.PI / 2),
      y: cy + r * Math.sin((2 * Math.PI * i) / n - Math.PI / 2),
    };
  });
  return result;
}

/** Convert screen client coords to SVG data coords using the element's current CTM. */
function clientToSvg(svg: SVGSVGElement, clientX: number, clientY: number): { x: number; y: number } {
  const pt = svg.createSVGPoint();
  pt.x = clientX;
  pt.y = clientY;
  const ctm = svg.getScreenCTM();
  if (!ctm) return { x: 0, y: 0 };
  const p = pt.matrixTransform(ctm.inverse());
  return { x: p.x, y: p.y };
}

// ─── Component ────────────────────────────────────────────────────────────────

interface GraphCanvasProps {
  vertices: CanvasVertex[];
  edges: CanvasEdge[];
  /** vertex id → CSS color — enables colored + glowing rendering */
  vertexColors?: Record<string, string>;
  /** edge label → CSS color — enables colored + glowing rendering */
  edgeColors?: Record<string, string>;
  /** Allow user to drag vertices to rearrange layout (positions not saved). Default: true */
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

  // Positions maintained internally; reset when vertex set changes
  const vertexKey = vertices.map((v) => v.id).join(',');
  const [positions, setPositions] = useState<Record<string, { x: number; y: number }>>(() =>
    circleLayoutPositions(vertices.map((v) => v.id)),
  );
  useEffect(() => {
    setPositions(circleLayoutPositions(vertices.map((v) => v.id)));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [vertexKey]);

  // ViewBox state + always-current ref for native handlers
  const [viewBox, setViewBox] = useState<ViewBox>(INIT_VB);
  const viewBoxRef = useRef<ViewBox>(INIT_VB);
  viewBoxRef.current = viewBox;

  // Positions ref for access inside handlers without closure staleness
  const positionsRef = useRef(positions);
  positionsRef.current = positions;

  // Drag state
  const dragging = useRef<string | null>(null);
  const dragOffset = useRef({ dx: 0, dy: 0 }); // offset from vertex center to cursor at drag start

  // Pan state: captured at mousedown, used throughout the pan gesture
  const panStart = useRef<{
    clientX: number; clientY: number;
    vbX: number; vbY: number;
    scaleX: number; scaleY: number;
  } | null>(null);

  // ── Coordinate helper ─────────────────────────────────────────────────────

  const getSvgCoords = useCallback((clientX: number, clientY: number) => {
    const svg = svgRef.current;
    if (!svg) return { x: 0, y: 0 };
    return clientToSvg(svg, clientX, clientY);
  }, []);

  // ── Native wheel handler (passive:false required for preventDefault to work) ─

  useEffect(() => {
    const svg = svgRef.current;
    if (!svg) return;
    const onWheel = (e: WheelEvent) => {
      e.preventDefault();
      const factor = e.deltaY > 0 ? 1.15 : 0.87;
      const { x: cx, y: cy } = clientToSvg(svg, e.clientX, e.clientY);
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
    svg.addEventListener('wheel', onWheel, { passive: false });
    return () => svg.removeEventListener('wheel', onWheel);
  }, []);

  // ── Zoom buttons ──────────────────────────────────────────────────────────

  const applyZoom = useCallback((factor: number) => {
    setViewBox((vb) => {
      const cx = vb.x + vb.w / 2;
      const cy = vb.y + vb.h / 2;
      const newW = Math.min(MAX_VB_W, Math.max(MIN_VB_W, vb.w * factor));
      const sf = newW / vb.w;
      return { x: cx - (cx - vb.x) * sf, y: cy - (cy - vb.y) * sf, w: newW, h: (newW / SVG_W) * SVG_H };
    });
  }, []);

  const handleZoomIn = () => applyZoom(0.87);
  const handleZoomOut = () => applyZoom(1.15);
  const handleReset = () => setViewBox(INIT_VB);

  // ── Vertex drag ───────────────────────────────────────────────────────────

  const handleVertexMouseDown = (e: React.MouseEvent, id: string) => {
    if (!movable) return;
    e.stopPropagation();
    e.preventDefault(); // prevent browser text-selection drag
    dragging.current = id;
    panStart.current = null; // not a pan

    // Capture offset so vertex center stays under cursor (no snap)
    const svgPt = getSvgCoords(e.clientX, e.clientY);
    const pos = positionsRef.current[id];
    dragOffset.current = pos
      ? { dx: svgPt.x - pos.x, dy: svgPt.y - pos.y }
      : { dx: 0, dy: 0 };
  };

  // ── Background drag (pan) ─────────────────────────────────────────────────

  const handleSvgMouseDown = (e: React.MouseEvent<SVGSVGElement>) => {
    // Only start pan when clicking the background (svg or transparent rect), not a vertex
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

  // ── Mouse move ────────────────────────────────────────────────────────────

  const handleMouseMove = useCallback(
    (e: React.MouseEvent) => {
      // Pan
      if (panStart.current && !dragging.current) {
        const p = panStart.current;
        setViewBox((vb) => ({
          ...vb,
          x: p.vbX - (e.clientX - p.clientX) * p.scaleX,
          y: p.vbY - (e.clientY - p.clientY) * p.scaleY,
        }));
        return;
      }

      // Vertex drag
      if (dragging.current) {
        const { x, y } = getSvgCoords(e.clientX, e.clientY);
        const id = dragging.current;
        setPositions((prev) => ({
          ...prev,
          [id]: { x: x - dragOffset.current.dx, y: y - dragOffset.current.dy },
        }));
      }
    },
    [getSvgCoords],
  );

  const handleMouseUp = () => {
    dragging.current = null;
    panStart.current = null;
  };

  // ── Render ────────────────────────────────────────────────────────────────

  const vbStr = `${viewBox.x} ${viewBox.y} ${viewBox.w} ${viewBox.h}`;

  return (
    <div
      className="relative w-full rounded border"
      style={{ borderColor: '#d5dbdb', userSelect: 'none' }}
    >
      {/* Zoom controls — top-right corner */}
      <div className="absolute right-2 top-2 z-10 flex flex-col gap-1">
        {[
          { label: '+', title: 'Zoom in', onClick: handleZoomIn },
          { label: '−', title: 'Zoom out', onClick: handleZoomOut },
          { label: '⤢', title: 'Reset zoom', onClick: handleReset },
        ].map(({ label, title, onClick }) => (
          <button
            key={label}
            onClick={onClick}
            title={title}
            className="flex h-7 w-7 cursor-pointer items-center justify-center rounded text-sm font-bold"
            style={{ background: 'rgba(255,255,255,0.92)', border: '1px solid #d5dbdb', color: '#16191f' }}
          >
            {label}
          </button>
        ))}
      </div>

      <svg
        ref={svgRef}
        viewBox={vbStr}
        style={{
          width: '100%',
          height,
          background: '#f2f3f3',
          display: 'block',
          cursor: panStart.current ? 'grabbing' : movable ? 'grab' : 'default',
          userSelect: 'none',
        }}
        onMouseDown={handleSvgMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseUp}
      >
        {/* Transparent background rect to catch mouse events */}
        <rect width={SVG_W} height={SVG_H} fill="transparent" />

        {/* Edges */}
        {edges.map((edge) => {
          const u = positions[edge.u];
          const v = positions[edge.v];
          if (!u || !v) return null;
          const mx = (u.x + v.x) / 2;
          const my = (u.y + v.y) / 2;
          const color = edgeColors?.[edge.label];
          return (
            <g key={edge.label}>
              <line
                x1={u.x} y1={u.y} x2={v.x} y2={v.y}
                stroke={color ?? '#aab7b8'}
                strokeWidth={color ? 3 : 2}
                style={color ? { filter: `drop-shadow(0 0 5px ${color})` } : undefined}
              />
              <text
                x={mx} y={my - 7}
                textAnchor="middle" fontSize={10} fill="#879596"
                style={{ userSelect: 'none', pointerEvents: 'none' }}
              >
                {edge.label}
              </text>
            </g>
          );
        })}

        {/* Vertices */}
        {vertices.map((v) => {
          const pos = positions[v.id];
          if (!pos) return null;
          const color = vertexColors?.[v.id];
          return (
            <g
              key={v.id}
              transform={`translate(${pos.x},${pos.y})`}
              onMouseDown={(e) => handleVertexMouseDown(e, v.id)}
              style={{ cursor: movable ? 'grab' : 'default' }}
            >
              <circle
                r={VERTEX_R}
                fill="white"
                stroke={color ?? '#ff9900'}
                strokeWidth={color ? 3 : 2}
                style={color ? { filter: `drop-shadow(0 0 6px ${color})` } : undefined}
              />
              <text
                textAnchor="middle" dominantBaseline="middle"
                fontSize={11} fill="#16191f"
                style={{ userSelect: 'none', pointerEvents: 'none' }}
              >
                {v.id.length > 5 ? v.id.slice(0, 4) + '…' : v.id}
              </text>
            </g>
          );
        })}
      </svg>

      {vertices.length === 0 && (
        <div
          className="absolute inset-0 flex items-center justify-center"
          style={{ pointerEvents: 'none' }}
        >
          <span className="text-sm" style={{ color: '#879596' }}>No vertices</span>
        </div>
      )}
    </div>
  );
}
