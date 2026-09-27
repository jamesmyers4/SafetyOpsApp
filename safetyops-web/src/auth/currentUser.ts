import { createContext, useContext } from 'react';
import type { CurrentUser } from '../services/api';

export const CurrentUserContext = createContext<CurrentUser | null>(null);

/** The signed-in user. Only available inside routes wrapped by RequireAuth. */
export function useCurrentUser(): CurrentUser | null {
    return useContext(CurrentUserContext);
}

/** Where to go after signing in: only same-site paths, so a crafted link can't bounce the user elsewhere. */
export function safeReturnUrl(value: string | null): string {
    return value && value.startsWith('/') && !value.startsWith('//') ? value : '/home';
}
