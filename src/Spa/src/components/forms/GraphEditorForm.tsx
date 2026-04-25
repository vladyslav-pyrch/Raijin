import {useCallback, useRef, useState} from 'react';
import {Button} from '../Button';
import {Spinner} from '../Spinner';

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
  onSubmit: (
    vertices: string[],
    edges: { label: string; u: string; v: string }[],
    colorCount: number,
  ) => Promise<void>;
  loading: boolean;
}

type Mode = 'add' | 'move';

const SVG_W = 600;
const SVG_H = 380;
const VERTEX_R = 22;

let uidCounter = 0;
function uid() {
  return `v${++uidCounter}`;
}

export function GraphEditorForm({ onSubmit, loading }: GraphEditorFormProps) {
  const [vertices, setVertices] = useState<GraphVertex[]>([]);
  const [edges, setEdges] = useState<GraphEdge[]>([]);
  const [colorCount, setColorCount] = useState(3);
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
    await onSubmit(
      vertices.map((v) => v.name),
      edges,
      colorCount,
    );
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <span className="text-sm font-medium text-gray-700">Mode:</span>
        <button
          onClick={() => setMode('add')}
          className={`px-3 py-1 rounded-lg text-sm font-medium border cursor-pointer transition-colors ${mode === 'add' ? 'bg-orange-500 text-white border-orange-500' : 'bg-white text-gray-600 border-gray-300 hover:bg-gray-50'}`}
        >
          Add
        </button>
        <button
          onClick={() => setMode('move')}
          className={`px-3 py-1 rounded-lg text-sm font-medium border cursor-pointer transition-colors ${mode === 'move' ? 'bg-orange-500 text-white border-orange-500' : 'bg-white text-gray-600 border-gray-300 hover:bg-gray-50'}`}
        >
          Move
        </button>
        <span className="text-xs text-gray-400 ml-2">
          {mode === 'add'
            ? 'Click canvas → add vertex. Drag vertex → vertex = add edge. Right-click = delete.'
            : 'Drag vertices to reposition.'}
        </span>
      </div>

      <svg
        ref={svgRef}
        viewBox={`0 0 ${SVG_W} ${SVG_H}`}
        className="w-full border border-gray-200 rounded-lg bg-gray-50 cursor-crosshair select-none"
        style={{ height: 320 }}
        onClick={handleSvgClick}
        onMouseMove={handleSvgMouseMove}
        onMouseUp={handleSvgMouseUp}
        onMouseLeave={handleSvgMouseUp}
      >
        {/* background rect to catch clicks */}
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
              <line
                x1={u.x} y1={u.y} x2={v.x} y2={v.y}
                stroke="#d1d5db" strokeWidth={2}
              />
              {/* wide transparent hit area for right-click */}
              <line
                x1={u.x} y1={u.y} x2={v.x} y2={v.y}
                stroke="transparent" strokeWidth={16}
                onContextMenu={(e) => handleEdgeContextMenu(e, edge.label)}
                style={{ cursor: 'context-menu' }}
              />
              <text
                x={mx} y={my - 6}
                textAnchor="middle"
                fontSize={10}
                fill="#9ca3af"
              >
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
            stroke="#f97316" strokeWidth={1.5} strokeDasharray="6 3"
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
            <circle
              r={VERTEX_R}
              fill="white"
              stroke="#f97316"
              strokeWidth={2}
            />
            <text
              textAnchor="middle"
              dominantBaseline="middle"
              fontSize={11}
              fill="#374151"
              style={{ userSelect: 'none', pointerEvents: 'none' }}
            >
              {v.name.length > 5 ? v.name.slice(0, 4) + '…' : v.name}
            </text>
          </g>
        ))}
      </svg>

      <div className="flex items-center gap-4">
        <label className="text-sm font-medium text-gray-700">Color count:</label>
        <input
          type="number"
          min={1}
          value={colorCount}
          onChange={(e) => setColorCount(Number(e.target.value))}
          className="w-20 border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-orange-400"
          disabled={loading}
        />
        <span className="text-xs text-gray-400">
          {vertices.length} vertices · {edges.length} edges
        </span>
      </div>

      {error && <p className="text-red-500 text-sm">{error}</p>}

      <div className="flex justify-end">
        <Button variant="primary" onClick={handleSubmit} disabled={loading}>
          {loading && <Spinner size="sm" />}
          Set Instance
        </Button>
      </div>
    </div>
  );
}
