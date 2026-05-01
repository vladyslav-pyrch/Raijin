export function DescriptionSection({description}: { description: string }) {
    return (
        <section className="card">
            <div className="card-header">
                <h2 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">
                    Description
                </h2>
            </div>
            <div className="px-4 py-4">
                {description.trim() ? (
                    <pre
                        className="text-sm whitespace-pre-wrap font-geist leading-relaxed text-neutral-900 dark:text-neutral-100">
            {description}
          </pre>
                ) : (
                    <p className="text-sm italic text-neutral-400 dark:text-neutral-500">
                        No description provided.
                    </p>
                )}
            </div>
        </section>
    );
}
