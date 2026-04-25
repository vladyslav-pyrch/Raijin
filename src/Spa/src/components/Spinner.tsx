type Size = 'sm' | 'md' | 'lg';

const sizeClasses: Record<Size, string> = {
  sm: 'w-4 h-4 border-2',
  md: 'w-6 h-6 border-2',
  lg: 'w-8 h-8 border-3',
};

export function Spinner({ size = 'md' }: { size?: Size }) {
  return (
    <span
      className={`inline-block rounded-full border-gray-300 border-t-orange-500 animate-spin ${sizeClasses[size]}`}
      aria-label="Loading"
    />
  );
}
