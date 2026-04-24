import {useEffect} from 'react';
import {createPortal} from 'react-dom';

interface ModalProps {
  open: boolean;
  title: string;
  onClose: () => void;
  children: React.ReactNode;
}

export function Modal({ open, title, onClose, children }: ModalProps) {
  useEffect(() => {
    if (!open) return;
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [open, onClose]);

  if (!open) return null;

  return createPortal(
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      onMouseDown={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <div className="bg-white border border-[#d5dbdb] shadow-2xl w-full max-w-xl mx-4 overflow-hidden rounded">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-3 bg-[#f2f3f3] border-b border-[#d5dbdb]">
          <h2 className="text-sm font-semibold text-[#16191f]">{title}</h2>
          <button
            onClick={onClose}
            className="text-[#545b64] hover:text-[#16191f] text-xl leading-none cursor-pointer w-6 h-6 flex items-center justify-center"
            aria-label="Close"
          >
            ×
          </button>
        </div>
        <div className="px-5 py-5">{children}</div>
      </div>
    </div>,
    document.body,
  );
}
