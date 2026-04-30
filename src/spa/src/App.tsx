import {Route, Routes, useNavigate, useParams} from 'react-router-dom';
import {useProblems} from './hooks/useProblems';
import {Button} from './components/Button';
import {Spinner} from './components/Spinner';
import {ProblemCard} from './components/ProblemCard';
import {WelcomePage} from './pages/WelcomePage';
import {ProblemDetailPage} from './pages/ProblemDetailPage';
import {CreateProblemPage} from './pages/CreateProblemPage';
import {EditProblemPage} from './pages/EditProblemPage';
import {HelpPage} from './pages/HelpPage';

// ─── Sidebar card list (needs useParams from router context) ──────────────────

function SidebarCardList({
  problems,
  onCardClick,
}: {
  problems: ReturnType<typeof useProblems>['problems'];
  onCardClick: (id: string) => void;
}) {
  const params = useParams<{ id: string }>();
  return (
    <>
      {problems.map((p) => (
        <ProblemCard
          key={p.id}
          problem={p}
          active={params.id === p.id}
          onClick={() => onCardClick(p.id)}
        />
      ))}
    </>
  );
}

// ─── Main layout ──────────────────────────────────────────────────────────────

function AppLayout() {
  const navigate = useNavigate();
  const { problems, loading, error, totalPages, page, loadPage, refresh } = useProblems();

  return (
    <div className="flex flex-col h-screen" style={{ background: '#f2f3f3' }}>
      {/* Top navigation bar */}
      <header
        className="sticky top-0 z-10 flex items-center shrink-0 min-h-[48px] px-4 gap-4"
        style={{ background: '#232f3e' }}
      >
        <button
          onClick={() => navigate('/')}
          className="text-base font-bold tracking-tight cursor-pointer hover:opacity-80 bg-transparent border-0 p-0"
          style={{ color: '#ff9900' }}
        >
          ⚡ Raijin
        </button>
        <span className="text-xs" style={{ color: '#d5dbdb', opacity: 0.6 }}>
          Combinatorics Solver
        </span>
        <div className="flex-1" />
        <button
          onClick={() => navigate('/help')}
          className="text-xs cursor-pointer hover:opacity-80"
          style={{ color: '#d5dbdb' }}
        >
          Help
        </button>
      </header>

      {/* Body */}
      <div className="flex flex-1 overflow-hidden">
        {/* Sidebar */}
        <aside
          className="w-64 shrink-0 flex flex-col overflow-hidden border-r"
          style={{ background: '#ffffff', borderColor: '#d5dbdb' }}
        >
          {/* Sidebar header */}
          <div
            className="px-3 py-2 border-b flex items-center justify-between shrink-0"
            style={{ borderColor: '#d5dbdb', background: '#fafafa' }}
          >
            <span className="text-xs font-semibold uppercase tracking-wider" style={{ color: '#545b64' }}>
              Problems
            </span>
            <button
              onClick={() => navigate('/create')}
              className="text-xs font-semibold px-2 py-1 rounded cursor-pointer transition-colors"
              style={{
                background: '#ff9900',
                color: '#16191f',
                border: '1px solid #e88b00',
              }}
              onMouseEnter={(e) => (e.currentTarget.style.background = '#e88b00')}
              onMouseLeave={(e) => (e.currentTarget.style.background = '#ff9900')}
            >
              + Create
            </button>
          </div>

          {/* Problem list */}
          <div className="flex-1 overflow-y-auto">
            {loading && (
              <div className="flex justify-center py-8">
                <Spinner />
              </div>
            )}
            {error && (
              <p className="text-xs px-3 py-2" style={{ color: '#d13212' }}>
                {error}
              </p>
            )}
            {!loading && problems.length === 0 && (
              <p className="text-xs text-center py-8" style={{ color: '#879596' }}>
                No problems yet
              </p>
            )}
            <Routes>
              <Route
                path="*"
                element={
                  <SidebarCardList
                    problems={problems}
                    onCardClick={(id) => navigate(`/problems/${id}`)}
                  />
                }
              />
            </Routes>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div
              className="flex items-center justify-between px-3 py-2 border-t text-xs shrink-0"
              style={{ borderColor: '#d5dbdb' }}
            >
              <Button size="sm" variant="ghost" onClick={() => loadPage(page - 1)} disabled={page <= 1}>
                ‹ Prev
              </Button>
              <span style={{ color: '#879596' }}>
                {page} / {totalPages}
              </span>
              <Button size="sm" variant="ghost" onClick={() => loadPage(page + 1)} disabled={page >= totalPages}>
                Next ›
              </Button>
            </div>
          )}
        </aside>

        {/* Main content */}
        <main className="flex-1 overflow-y-auto" style={{ background: '#f2f3f3' }}>
          <Routes>
            <Route path="/" element={<WelcomePage />} />
            <Route path="/help" element={<HelpPage />} />
            <Route
              path="/create"
              element={<CreateProblemPage onProblemChanged={refresh} />}
            />
            <Route
              path="/problems/:id"
              element={<ProblemDetailPage onProblemChanged={refresh} />}
            />
            <Route
              path="/problems/:id/edit"
              element={<EditProblemPage onProblemChanged={refresh} />}
            />
          </Routes>
        </main>
      </div>

      {/* Footer */}
      <footer
        className="shrink-0 min-h-[36px] border-t flex items-center px-4"
        style={{ background: '#fafafa', borderColor: '#d5dbdb' }}
      />
    </div>
  );
}

export default AppLayout;
