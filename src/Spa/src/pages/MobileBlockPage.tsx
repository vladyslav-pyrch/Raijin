export function MobileBlockPage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen px-8 text-center bg-neutral-50 dark:bg-surface">
      <div className="max-w-xs space-y-6">
        {/* Icon */}
        <div className="flex items-center justify-center gap-3 text-5xl select-none">
          <span>💻</span>
        </div>

        {/* Heading */}
        <div className="space-y-2">
          <h1 className="text-xl font-semibold text-neutral-900 dark:text-neutral-100">
            Desktop only
          </h1>
          <p className="text-sm leading-relaxed text-neutral-500 dark:text-neutral-400">
            Raijin is designed for desktop use. Please open it on a PC or laptop for the best
            experience.
          </p>
        </div>

        {/* Divider */}
        <div className="border-t border-neutral-200 dark:border-neutral-700" />

        {/* Detail */}
        <p className="text-xs text-neutral-400 dark:text-neutral-500">
          The interactive graph editor, solution tables, and sidebar layout require a larger
          screen.
        </p>
      </div>
    </div>
  );
}
