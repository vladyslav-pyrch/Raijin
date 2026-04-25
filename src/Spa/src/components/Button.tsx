import type {ButtonHTMLAttributes} from 'react';

type Variant = 'primary' | 'secondary' | 'danger' | 'ghost' | 'link';
type Size = 'sm' | 'md';

const variantClasses: Record<Variant, string> = {
  primary:
    'bg-[#ff9900] hover:bg-[#e88b00] text-[#16191f] border border-[#e88b00] font-semibold',
  secondary:
    'bg-white hover:bg-[#f2f3f3] text-[#16191f] border border-[#aab7b8] hover:border-[#879596]',
  danger:
    'bg-[#d13212] hover:bg-[#ba2e0f] text-white border border-[#ba2e0f]',
  ghost:
    'bg-transparent hover:bg-[#f2f3f3] text-[#545b64] border border-transparent hover:border-[#d5dbdb]',
  link:
    'bg-transparent text-[#0073bb] hover:text-[#005f99] hover:underline border border-transparent p-0',
};

const sizeClasses: Record<Size, string> = {
  sm: 'px-3 py-1 text-xs',
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
  return (
    <button
      {...rest}
      disabled={disabled}
      className={[
        'inline-flex items-center gap-1.5 rounded cursor-pointer transition-colors',
        variantClasses[variant],
        variant === 'link' ? '' : sizeClasses[size],
        disabled ? 'opacity-50 cursor-not-allowed' : '',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      {children}
    </button>
  );
}
