export function WelcomePage() {
  return (
    <div className="flex flex-col items-center justify-center h-full text-center px-8">
      <div className="max-w-sm">
        <div className="text-5xl mb-5">⚡</div>
        <h1 className="text-xl font-semibold mb-2" style={{ color: '#16191f' }}>
          Raijin Combinatorics Solver
        </h1>
        <p className="text-sm leading-relaxed" style={{ color: '#545b64' }}>
          Select a problem from the sidebar, or create a new one to get started.
        </p>
      </div>
    </div>
  );
}
