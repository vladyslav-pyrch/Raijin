import {useCallback, useRef, useState} from 'react';
import {Button} from '../Button';
import {Spinner} from '../Spinner';
import type {EdgeDto, GraphDto, VertexDto} from '../../services/combinatorics';

interface GraphVertex {
  id: string;
  name: string;
  x: number;
  y: number;
}

interface GraphEdge {
  label: string;
  u: string;
  v: string;
}

interface GraphEditorFormProps {
  onSubmit: (instance: { graph: GraphDto; colorCount: number }) => Promise<void>;
  loading: boolean;
  initialVertices?: GraphVertex[];
  initialEdges?: GraphEdge[];
  initialColorCount?: number;
}

type Mode = 'add' | 'move';

const SVG_W = 600;
const SVG_H = 380;
const VERTEX_R = 22;

let uidCounter = 0;
function uid() {
  return `v${++uidCounter}`;
}

/** Lay out vertices in a circle for pre-fill when no positions are known. */
export function circleLayout(ids: string[]): GraphVertex[] {
  const cx = SVG_W / 2;
  const cy = SVG_H / 2;
  const r = Math.min(SVG_W, SVG_H) * 0.35;
  return ids.map((id, i) => ({
    id: uid(),
    name: id,
    x: cx + r * Math.cos((2 * Math.PI * i) / ids.length - Math.PI / 2),
    y: cy + r * Math.sin((2 * Math.PI * i) / ids.length - Math.PI / 2),
  }));
}

export function GraphEditorForm({
  onSubmit,
  loading,
  initialVertices,
  initialEdges,
  initialColorCount,
}: GraphEditorFormProps) {
  const [vertices, setVertices] = useState<GraphVertex[]>(initialVertices ?? []);
  const [edges, setEdges] = useState<GraphEdge[]>(initialEdges ?? []);
  const [colorCount, setColorCount] = useState(initialColorCount ?? 3);
  const [mode, setMode] = useState<Mode>('add');
  const [error, setError] = useState<string | null>(null);

  // drag state
  const dragSource = useRef<string | null>(null);
  const movingVertex = useRef<string | null>(null);
  const [ghostLine, setGhostLine] = useState<{ x1: number; y1: number; x2: number; y2: number } | null>(null);

  const svgRef = useRef<SVGSVGElement>(null);

  const getSvgCoords = (e: React.MouseEvent): { x: number; y: number } => {
    const svg = svgRef.current;
    if (!svg) return { x: 0, y: 0 };
    const rect = svg.getBoundingClientRect();
    const scaleX = SVG_W / rect.width;
    const scaleY = SVG_H / rect.height;
    return {
      x: (e.clientX - rect.left) * scaleX,
      y: (e.clientY - rect.top) * scaleY,
    };
  };

  const handleSvgClick = (e: React.MouseEvent<SVGSVGElement>) => {
    if (mode !== 'add') return;
    if (e.target !== svgRef.current && (e.target as Element).tagName === 'circle') return;
    if ((e.target as Element).tagName !== 'svg' && (e.target as Element).tagName !== 'rect') return;

    const { x, y } = getSvgCoords(e);
    const name = window.prompt('Vertex name:');
    if (!name || !name.trim()) return;
    if (vertices.some((v) => v.name === name.trim())) {
      alert(`Vertex "${name.trim()}" already exists`);
      return;
    }
    setVertices((prev) => [...prev, { id: uid(), name: name.trim(), x, y }]);
  };

  const handleVertexMouseDown = (e: React.MouseEvent, vertexId: string) => {
    e.stopPropagation();
    if (mode === 'add') {
      dragSource.current = vertexId;
    } else {
      movingVertex.current = vertexId;
    }
  };

  const handleSvgMouseMove = (e: React.MouseEvent<SVGSVGElement>) => {
    if (mode === 'add' && dragSource.current) {
      const src = vertices.find((v) => v.id === dragSource.current);
      if (!src) return;
      const { x, y } = getSvgCoords(e);
      setGhostLine({ x1: src.x, y1: src.y, x2: x, y2: y });
    }
    if (mode === 'move' && movingVertex.current) {
      const { x, y } = getSvgCoords(e);
      setVertices((prev) =>
        prev.map((v) => (v.id === movingVertex.current ? { ...v, x, y } : v)),
      );
    }
  };

  const handleSvgMouseUp = useCallback(() => {
    dragSource.current = null;
    movingVertex.current = null;
    setGhostLine(null);
  }, []);

  const handleVertexMouseUp = (e: React.MouseEvent, targetId: string) => {
    e.stopPropagation();
    if (mode === 'add' && dragSource.current && dragSource.current !== targetId) {
      const srcId = dragSource.current;
      dragSource.current = null;
      setGhostLine(null);
      const label = window.prompt('Edge label:');
      if (!label || !label.trim()) return;
      if (edges.some((ed) => ed.label === label.trim())) {
        alert(`Edge "${label.trim()}" already exists`);
        return;
      }
      const srcV = vertices.find((v) => v.id === srcId);
      const tgtV = vertices.find((v) => v.id === targetId);
      if (!srcV || !tgtV) return;
      setEdges((prev) => [
        ...prev,
        { label: label.trim(), u: srcV.name, v: tgtV.name },
      ]);
    } else {
      dragSource.current = null;
      movingVertex.current = null;
      setGhostLine(null);
    }
  };

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

  const handleSubmit = async () => {
    setError(null);
    if (vertices.length === 0) {
      setError('Add at least one vertex');
      return;
    }
    if (colorCount < 1) {
      setError('Color count must be at least 1');
      return;
    }

    const graphVertices: VertexDto[] = vertices.map((v) => ({ id: v.name }));
    const graphEdges: EdgeDto[] = edges;

    await onSubmit({ graph: { vertices: graphVertices, edges: graphEdges }, colorCount });
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <span className="text-xs font-medium" style={{ color: '#545b64' }}>Mode:</span>
        <button
          onClick={() => setMode('add')}
          className="px-3 py-1 rounded text-xs font-medium border cursor-pointer transition-colors"
          style={
            mode === 'add'
              ? { background: '#ff9900', color: '#16191f', borderColor: '#e88b00' }
              : { background: '#fff', color: '#545b64', borderColor: '#aab7b8' }
          }
        >
          Add
        </button>
        <button
          onClick={() => setMode('move')}
          className="px-3 py-1 rounded text-xs font-medium border cursor-pointer transition-colors"
          style={
            mode === 'move'
              ? { background: '#ff9900', color: '#16191f', borderColor: '#e88b00' }
              : { background: '#fff', color: '#545b64', borderColor: '#aab7b8' }
          }
        >
          Move
        </button>
        <span className="text-xs" style={{ color: '#879596' }}>
          {mode === 'add'
            ? 'Click canvas → vertex. Drag vertex→vertex → edge. Right-click → delete.'
            : 'Drag vertices to reposition.'}
        </span>
      </div>

      <svg
        ref={svgRef}
        viewBox={`0 0 ${SVG_W} ${SVG_H}`}
        className="w-full border rounded cursor-crosshair select-none"
        style={{ height: 300, background: '#f2f3f3', borderColor: '#d5dbdb' }}
        onClick={handleSvgClick}
        onMouseMove={handleSvgMouseMove}
        onMouseUp={handleSvgMouseUp}
        onMouseLeave={handleSvgMouseUp}
      >
        <rect width={SVG_W} height={SVG_H} fill="transparent" />

        {/* edges */}
        {edges.map((edge) => {
          const u = vertices.find((v) => v.name === edge.u);
          const v = vertices.find((v) => v.name === edge.v);
          if (!u || !v) return null;
          const mx = (u.x + v.x) / 2;
          const my = (u.y + v.y) / 2;
          return (
            <g key={edge.label}>
              <line x1={u.x} y1={u.y} x2={v.x} y2={v.y} stroke="#d5dbdb" strokeWidth={2} />
              <line
                x1={u.x} y1={u.y} x2={v.x} y2={v.y}
                stroke="transparent" strokeWidth={16}
                onContextMenu={(e) => handleEdgeContextMenu(e, edge.label)}
                style={{ cursor: 'context-menu' }}
              />
              <text x={mx} y={my - 6} textAnchor="middle" fontSize={10} fill="#879596">
                {edge.label}
              </text>
            </g>
          );
        })}

        {/* ghost drag line */}
        {ghostLine && (
          <line
            x1={ghostLine.x1} y1={ghostLine.y1}
            x2={ghostLine.x2} y2={ghostLine.y2}
            stroke="#ff9900" strokeWidth={1.5} strokeDasharray="6 3"
          />
        )}

        {/* vertices */}
        {vertices.map((v) => (
          <g
            key={v.id}
            transform={`translate(${v.x},${v.y})`}
            onMouseDown={(e) => handleVertexMouseDown(e, v.id)}
            onMouseUp={(e) => handleVertexMouseUp(e, v.id)}
            onContextMenu={(e) => handleVertexContextMenu(e, v.id)}
            style={{ cursor: mode === 'move' ? 'grab' : 'pointer' }}
          >
            <circle r={VERTEX_R} fill="white" stroke="#ff9900" strokeWidth={2} />
            <text
              textAnchor="middle"
              dominantBaseline="middle"
              fontSize={11}
              fill="#16191f"
              style={{ userSelect: 'none', pointerEvents: 'none' }}
            >
              {v.name.length > 5 ? v.name.slice(0, 4) + '…' : v.name}
            </text>
          </g>
        ))}
      </svg>

      <div className="flex items-center gap-4">
        <label className="text-xs font-medium" style={{ color: '#545b64' }}>Color count:</label>
        <input
          type="number"
          min={1}
          value={colorCount}
          onChange={(e) => setColorCount(Number(e.target.value))}
          className="w-20 border rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-1"
          style={{ borderColor: '#aab7b8' }}
          disabled={loading}
        />
        <span className="text-xs" style={{ color: '#879596' }}>
          {vertices.length} vertices · {edges.length} edges
        </span>
      </div>

      {error && <p className="text-xs" style={{ color: '#d13212' }}>{error}</p>}

      <div className="flex justify-end">
        <Button variant="primary" onClick={handleSubmit} disabled={loading}>
          {loading && <Spinner size="sm" />}
          Create
        </Button>
      </div>
    </div>
  );
}
