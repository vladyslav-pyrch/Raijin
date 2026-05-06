export function WelcomePage() {
    return (
        <div className="flex flex-col items-center justify-center h-full text-center px-8">
            <div className="max-w-sm">
                <div className="text-5xl mb-5">⚡</div>
                <h1 className="text-xl font-semibold mb-2 text-neutral-900 dark:text-neutral-100">
                    Raijin Combinatorics Solver
                </h1>
                <p className="text-sm leading-relaxed text-neutral-500 dark:text-neutral-400">
                    Select a problem from the sidebar, or create a new one to get started.
                </p>
            </div>
        </div>
    );
}
