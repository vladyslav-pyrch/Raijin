import type {ButtonHTMLAttributes} from 'react';

type Variant = 'primary' | 'secondary' | 'danger' | 'ghost' | 'link';
type Size = 'sm' | 'md';

const variantClasses: Record<Variant, string> = {
    primary:
        'bg-primary-500 text-white hover:bg-primary-600 active:bg-primary-700 border border-primary-600',
    secondary:
        'bg-neutral-100 dark:bg-surface-secondary text-neutral-900 dark:text-neutral-100 border border-neutral-200 dark:border-neutral-700 hover:bg-neutral-200 dark:hover:bg-surface-tertiary',
    danger:
        'bg-error-500 text-white hover:bg-error-600 active:bg-error-700 border border-error-600',
    ghost:
        'bg-transparent text-neutral-600 dark:text-neutral-400 border border-transparent hover:bg-neutral-100 dark:hover:bg-surface-tertiary',
    link:
        'bg-transparent text-primary-500 dark:text-primary-400 hover:text-primary-600 dark:hover:text-primary-300 hover:underline border-0 p-0',
};

const sizeClasses: Record<Size, string> = {
    sm: 'px-2.5 py-1 text-xs',
    md: 'px-4 py-2 text-sm',
};

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: Variant;
    size?: Size;
}

export function Button({
                           variant = 'secondary',
                           size = 'md',
                           className = '',
                           disabled,
                           children,
                           ...rest
                       }: ButtonProps) {
    const isLink = variant === 'link';
    return (
        <button
            {...rest}
            disabled={disabled}
            className={[
                'inline-flex items-center gap-1.5 rounded-md font-medium transition-colors duration-150 cursor-pointer',
                'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
                'disabled:opacity-50 disabled:cursor-not-allowed',
                variantClasses[variant],
                isLink ? (size === 'sm' ? 'text-xs' : 'text-sm') : sizeClasses[size],
                className,
            ]
                .filter(Boolean)
                .join(' ')}
        >
            {children}
        </button>
    );
}
