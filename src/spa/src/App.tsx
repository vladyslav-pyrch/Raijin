import {useState} from 'react';
import {Route, Routes, useNavigate, useParams} from 'react-router-dom';
import {useProblems} from './hooks/useProblems';
import {useMobileDetect} from './hooks/useMobileDetect';
import {getCookie, setCookie} from './lib/cookies';
import {Button} from './components/Button';
import {Spinner} from './components/Spinner';
import {ProblemCard} from './components/ProblemCard';
import {WelcomePage} from './pages/WelcomePage';
import {ProblemDetailPage} from './pages/ProblemDetailPage';
import {CreateProblemPage} from './pages/CreateProblemPage';
import {EditProblemPage} from './pages/EditProblemPage';
import {HelpPage} from './pages/HelpPage';
import {MobileBlockPage} from './pages/MobileBlockPage';

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
  const isMobile = useMobileDetect();
  const navigate = useNavigate();
  const { problems, loading, error, totalPages, page, loadPage, refresh } = useProblems();

  const [darkMode, setDarkMode] = useState(
    () => getCookie('darkMode') === 'true',
  );

  const toggleDarkMode = () => {
    setDarkMode((prev) => {
      const next = !prev;
      document.documentElement.classList.toggle('dark', next);
      setCookie('darkMode', String(next));
      return next;
    });
  };

  if (isMobile) return <MobileBlockPage />;

  return (
    <div className="flex flex-col h-screen bg-neutral-50 dark:bg-surface text-neutral-900 dark:text-neutral-100">

      {/* Top navigation bar */}
      <header className="sticky top-0 z-10 flex items-center shrink-0 min-h-[48px] px-4 gap-4 bg-neutral-900 dark:bg-neutral-900 shadow-md">
        <button
          onClick={() => navigate('/')}
          className="text-base font-bold tracking-tight cursor-pointer hover:opacity-80 bg-transparent border-0 p-0 text-primary-400"
        >
          ⚡ Raijin
        </button>
        <span className="text-xs text-neutral-400 opacity-70">
          Combinatorics Solver
        </span>
        <div className="flex-1" />
        <button
          onClick={() => navigate('/help')}
          className="text-xs cursor-pointer text-neutral-400 hover:text-neutral-200 transition-colors"
        >
          Help
        </button>
        <button
          onClick={toggleDarkMode}
          title={darkMode ? 'Switch to light mode' : 'Switch to dark mode'}
          className="text-sm cursor-pointer text-neutral-400 hover:text-neutral-200 transition-colors w-7 h-7 flex items-center justify-center rounded hover:bg-neutral-700"
          aria-label="Toggle dark mode"
        >
          {darkMode ? '☀' : '☾'}
        </button>
      </header>

      {/* Body */}
      <div className="flex flex-1 overflow-hidden">

        {/* Sidebar */}
        <aside className="w-64 shrink-0 flex flex-col overflow-hidden border-r border-neutral-200 dark:border-neutral-700 bg-white dark:bg-surface-secondary">

          {/* Sidebar header */}
          <div className="px-3 py-2 border-b border-neutral-200 dark:border-neutral-700 bg-neutral-50 dark:bg-surface-tertiary flex items-center justify-between shrink-0">
            <span className="text-xs font-semibold uppercase tracking-wider text-neutral-500 dark:text-neutral-400">
              Problems
            </span>
            <button
              onClick={() => navigate('/create')}
              className="btn btn-primary btn-sm"
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
              <p className="text-xs px-3 py-2 text-error-500">{error}</p>
            )}
            {!loading && problems.length === 0 && (
              <p className="text-xs text-center py-8 text-neutral-400">
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
            <div className="flex items-center justify-between px-3 py-2 border-t border-neutral-200 dark:border-neutral-700 text-xs shrink-0">
              <Button size="sm" variant="ghost" onClick={() => loadPage(page - 1)} disabled={page <= 1}>
                ‹ Prev
              </Button>
              <span className="text-neutral-400">
                {page} / {totalPages}
              </span>
              <Button size="sm" variant="ghost" onClick={() => loadPage(page + 1)} disabled={page >= totalPages}>
                Next ›
              </Button>
            </div>
          )}
        </aside>

        {/* Main content */}
        <main className="flex-1 overflow-y-auto bg-neutral-50 dark:bg-surface">
          <Routes>
            <Route path="/" element={<WelcomePage />} />
            <Route path="/help" element={<HelpPage />} />
            <Route path="/create" element={<CreateProblemPage onProblemChanged={refresh} />} />
            <Route path="/problems/:id" element={<ProblemDetailPage onProblemChanged={refresh} />} />
            <Route path="/problems/:id/edit" element={<EditProblemPage onProblemChanged={refresh} />} />
          </Routes>
        </main>
      </div>

      {/* Footer */}
      <footer className="shrink-0 min-h-[36px] border-t border-neutral-200 dark:border-neutral-700 bg-white dark:bg-surface-secondary" />
    </div>
  );
}

export default AppLayout;
