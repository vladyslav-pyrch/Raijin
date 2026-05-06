import {useEffect, useState} from 'react';

/**
 * Returns true when the viewport is considered "mobile":
 *  - primary pointer is coarse (touch device), OR
 *  - viewport width < 1024 px
 *
 * Reacts to window resize so portrait↔landscape flips work correctly.
 */
export function useMobileDetect(): boolean {
    const query = '(pointer: coarse), (max-width: 1023px)';

    const [isMobile, setIsMobile] = useState<boolean>(() => {
        if (typeof window === 'undefined') return false;
        return window.matchMedia(query).matches;
    });

    useEffect(() => {
        const mq = window.matchMedia(query);
        const handler = (e: MediaQueryListEvent) => setIsMobile(e.matches);
        mq.addEventListener('change', handler);
        setIsMobile(mq.matches); // sync on mount in case it changed
        return () => mq.removeEventListener('change', handler);
    }, []); // query is a constant literal — no dep needed

    return isMobile;
}
