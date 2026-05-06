// ─── Help Page ────────────────────────────────────────────────────────────────

interface SectionProps {
    title: string;
    children: React.ReactNode;
}

function Section({title, children}: SectionProps) {
    return (
        <section className="card">
            <div className="card-header">
                <h2 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">{title}</h2>
            </div>
            <div className="px-5 py-4 text-sm text-neutral-900 dark:text-neutral-100">
                {children}
            </div>
        </section>
    );
}

function Code({children}: { children: React.ReactNode }) {
    return (
        <code
            className="rounded px-1.5 py-0.5 text-xs font-geist-mono bg-neutral-100 dark:bg-surface-tertiary text-neutral-900 dark:text-neutral-100 border border-neutral-200 dark:border-neutral-700">
            {children}
        </code>
    );
}

function Pre({children}: { children: string }) {
    return (
        <pre className="code-block rounded-md px-4 py-3 text-xs whitespace-pre overflow-auto">
      {children}
    </pre>
    );
}

export function HelpPage() {
    return (
        <div className="flex flex-col h-full">
            {/* Header */}
            <div
                className="shrink-0 border-b border-neutral-200 dark:border-neutral-700 px-6 py-4 bg-white dark:bg-surface-secondary">
                <p className="text-xs mb-1 text-neutral-400 dark:text-neutral-500">Help</p>
                <h1 className="text-lg font-semibold text-neutral-900 dark:text-neutral-100">
                    Raijin — Help &amp; Reference
                </h1>
            </div>

            <div className="flex-1 overflow-y-auto px-6 py-5 space-y-4 w-full">
                {/* Overview */}
                <Section title="Overview">
                    <p className="mb-2 text-neutral-500 dark:text-neutral-400">
                        Raijin is a combinatorics solver. You create a <strong>problem</strong>, set its
                        instance data, then reduce it to a SAT formula and solve it.
                    </p>
                    <ol className="list-decimal list-inside space-y-1 text-neutral-500 dark:text-neutral-400">
                        <li>Create a problem via <strong>+ Create</strong> in the sidebar.</li>
                        <li>Choose the problem type and fill in the instance.</li>
                        <li>Open the problem and click <strong>Solve</strong> (choose a solver).</li>
                        <li>Once completed, the solution and DIMACS encoding appear on the page.</li>
                    </ol>
                </Section>

                {/* Problem statuses */}
                <Section title="Problem Statuses">
                    <table className="w-full text-sm">
                        <thead className="table-header">
                        <tr>
                            <th className="text-left py-2 pr-4 font-medium text-xs text-neutral-500 dark:text-neutral-400">Status</th>
                            <th className="text-left py-2 font-medium text-xs text-neutral-500 dark:text-neutral-400">Meaning</th>
                        </tr>
                        </thead>
                        <tbody className="text-xs">
                        {[
                            ['NoSatEncoding', '#9CA3AF', 'No instance set or not yet reduced to SAT.'],
                            ['Pending', '#FF9933', 'Reduction to SAT queued; waiting for solver.'],
                            ['Running', '#0066CC', 'SAT solver is actively running.'],
                            ['Completed', '#10A760', 'Solver finished. Solution is available.'],
                            ['Failed', '#DC3545', 'Solver encountered an error.'],
                            ['TimedOut', '#FF6D2A', 'Solver exceeded the time limit.'],
                        ].map(([status, color, desc]) => (
                            <tr key={status} className="table-row">
                                <td className="py-2 pr-4">
                    <span
                        className="inline-block px-2 py-0.5 rounded font-semibold"
                        style={{background: `${color}22`, color, border: `1px solid ${color}`}}
                    >
                      {status}
                    </span>
                                </td>
                                <td className="py-2 text-neutral-500 dark:text-neutral-400">{desc}</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </Section>

                {/* Boolean Expression */}
                <Section title="Problem Type: Boolean Expression">
                    <p className="mb-2 text-neutral-500 dark:text-neutral-400">
                        A propositional logic formula over named variables. Supported operators:
                    </p>
                    <table className="w-full text-xs mb-3">
                        <thead className="table-header">
                        <tr>
                            <th className="text-left py-1.5 pr-4 font-medium text-neutral-500 dark:text-neutral-400">Operator</th>
                            <th className="text-left py-1.5 font-medium text-neutral-500 dark:text-neutral-400">Syntax</th>
                        </tr>
                        </thead>
                        <tbody>
                        {[
                            ['NOT', '~x  or  !x'],
                            ['AND', 'x & y  or  x && y or x * y'],
                            ['OR', 'x | y  or  x || y or x + y'],
                            ['XOR', 'x ^ y'],
                            ['IMPLIES', 'x => y  or  x -> y'],
                            ['EQUIV', 'x <=> y or x = y or x <-> y'],
                            ['Grouping', '(x | y) & z'],
                        ].map(([op, syn]) => (
                            <tr key={op} className="table-row">
                                <td className="py-1.5 pr-4 font-semibold text-neutral-900 dark:text-neutral-100">{op}</td>
                                <td className="py-1.5 font-geist-mono text-primary-500 dark:text-primary-400">{syn}</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                    <p className="text-xs mb-1 text-neutral-500 dark:text-neutral-400">Example:</p>
                    <Pre>{'(x1 | ~x2) & (x2 | x3) & (~x1 | ~x3)'}</Pre>
                </Section>

                {/* SAT */}
                <Section title="Problem Type: Boolean Satisfiability (SAT)">
                    <p className="mb-2 text-neutral-500 dark:text-neutral-400">
                        Input in <strong>DIMACS-like text format</strong>: one clause per line, literals
                        separated by spaces. Negate a literal with <Code>~</Code> prefix.
                    </p>
                    <p className="text-xs mb-1 text-neutral-500 dark:text-neutral-400">Example:</p>
                    <Pre>{'x1 ~x2 x3\n~x1 x4\nx2 ~x3'}</Pre>
                    <p className="text-xs mt-2 text-neutral-500 dark:text-neutral-400">
                        Each line is a disjunctive clause; the problem is satisfiable if all clauses can be
                        made true simultaneously.
                    </p>
                </Section>

                {/* CSP */}
                <Section title="Problem Type: Constraint Satisfaction Problem (CSP)">
                    <p className="mb-2 text-neutral-500 dark:text-neutral-400">
                        Define variables with discrete domains, then add constraints referencing those
                        variables.
                    </p>
                    <ul className="list-disc list-inside space-y-1 text-xs mb-3 text-neutral-500 dark:text-neutral-400">
                        <li>Variable name: alphanumeric string, e.g. <Code>color_A</Code></li>
                        <li>States (domain): comma-separated values, e.g. <Code>red, green, blue</Code></li>
                        <li>Constraint: an expression over variable names and their states</li>
                    </ul>
                    <p className="text-xs mb-1 text-neutral-500 dark:text-neutral-400">Example constraints:</p>
                    <Pre>{'~(color_A::red = color_B::red)\n' +
                        '~(color_A::green = color_B::green)\n' +
                        '~(color_A::blue = color_B::blue)\n' +
                        '~(color_B::red = color_C::red)\n' +
                        '~(color_B::green = color_C::green)\n' +
                        '~(color_B::blue = color_C::blue)\n' +
                        '~(color_A::red = color_C::red)\n' +
                        '~(color_A::green = color_C::green)\n' +
                        '~(color_A::blue = color_C::blue)'}</Pre>
                </Section>

                {/* Graph coloring */}
                <Section title="Problem Types: Vertex Coloring &amp; Edge Coloring">
                    <p className="mb-2 text-neutral-500 dark:text-neutral-400">
                        Both types use the interactive graph editor:
                    </p>
                    <ul className="list-disc list-inside space-y-1 text-xs mb-3 text-neutral-500 dark:text-neutral-400">
                        <li>
                            <strong>Add mode</strong> — click canvas to add a vertex (auto-named{' '}
                            <Code>v1</Code>, <Code>v2</Code>, …). Drag one vertex to another to create an edge
                            (auto-named <Code>e1</Code>, <Code>e2</Code>, …). Click a label to rename it.
                            Right-click to delete.
                        </li>
                        <li>
                            <strong>Move mode</strong> — drag vertices to rearrange positions.
                        </li>
                        <li>
                            <strong>Color count</strong> — maximum number of colors the solver may use.
                        </li>
                        <li>
                            Scroll / pinch to zoom; use <Code>+</Code> / <Code>−</Code> / <Code>⤢</Code>{' '}
                            buttons in the top-right corner.
                        </li>
                    </ul>
                    <p className="text-xs text-neutral-500 dark:text-neutral-400">
                        <strong>Vertex coloring</strong>: no two adjacent vertices share a color.
                        <br/>
                        <strong>Edge coloring</strong>: no two edges sharing a vertex share a color.
                    </p>
                </Section>

                {/* Action buttons */}
                <Section title="Action Buttons">
                    <table className="w-full text-xs">
                        <thead className="table-header">
                        <tr>
                            <th className="text-left py-1.5 pr-4 font-medium text-neutral-500 dark:text-neutral-400">Button</th>
                            <th className="text-left py-1.5 font-medium text-neutral-500 dark:text-neutral-400">Action</th>
                        </tr>
                        </thead>
                        <tbody>
                        {[
                            ['Check the Problem Status', 'Refresh all sections on the page.'],
                            ['Edit', 'Go to a page for updating the problem name and description.'],
                            ['New version', 'Open the create-problem page pre-filled with the current instance (for forking).'],
                            ['Solve', 'Submit the problem for SAT reduction and solving with the chosen solver (cadical or cryptominisat).'],
                            ['Show Variable Map', 'Lazily fetch and display the mapping from SAT variables to problem-domain names.'],
                        ].map(([btn, desc]) => (
                            <tr key={btn} className="table-row">
                                <td className="py-2 pr-4 font-semibold text-neutral-900 dark:text-neutral-100">{btn}</td>
                                <td className="py-2 text-neutral-500 dark:text-neutral-400">{desc}</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </Section>

                {/* DIMACS encoding */}
                <Section title="DIMACS Encoding">
                    <p className="mb-2 text-neutral-500 dark:text-neutral-400">
                        After reduction, the DIMACS section shows the CNF formula fed to the SAT solver:
                    </p>
                    <ul className="list-disc list-inside space-y-1 text-xs text-neutral-500 dark:text-neutral-400">
                        <li><strong>Variables</strong> — total number of Boolean variables.</li>
                        <li><strong>Clauses</strong> — total number of disjunctive clauses.</li>
                        <li>Table rows — each row is one clause; positive integer = variable, negative = negation.</li>
                        <li>Use the pagination controls to browse clauses (100 per page).</li>
                    </ul>
                </Section>

                {/* Solution + graph */}
                <Section title="Solution Display (Graph Problems)">
                    <p className="mb-2 text-neutral-500 dark:text-neutral-400">
                        For vertex / edge coloring problems a canvas renders the graph with solution colors:
                    </p>
                    <ul className="list-disc list-inside space-y-1 text-xs text-neutral-500 dark:text-neutral-400">
                        <li>Each color number maps to a distinct highlight color on vertices / edges.</li>
                        <li>Click the color swatch next to a number to change it via the browser color picker.</li>
                        <li>Drag vertices to rearrange the layout (positions are not saved).</li>
                        <li>Scroll / pinch or use the zoom buttons to navigate the canvas.</li>
                    </ul>
                </Section>
            </div>
        </div>
    );
}
