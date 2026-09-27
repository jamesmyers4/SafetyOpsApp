import { useEffect, useRef } from 'react';
import type { AppMessage } from '../types/messages';

// Frames and popups are always same-origin, so messages are only sent to, and only
// accepted from, this app's own origin.

export function postToParent(message: AppMessage): void {
    window.parent.postMessage(message, window.location.origin);
}

export function postToOpener(message: AppMessage): void {
    window.opener?.postMessage(message, window.location.origin);
}

/** Calls `onMessage` for every AppMessage this window receives from its own origin. */
export function useAppMessages(onMessage: (message: AppMessage) => void): void {
    const handlerRef = useRef(onMessage);
    useEffect(() => {
        handlerRef.current = onMessage;
    });

    useEffect(() => {
        function listener(event: MessageEvent) {
            if (event.origin !== window.location.origin) return;
            const data = event.data as AppMessage | null;
            if (data && typeof data === 'object' && typeof data.type === 'string') handlerRef.current(data);
        }
        window.addEventListener('message', listener);
        return () => window.removeEventListener('message', listener);
    }, []);
}
