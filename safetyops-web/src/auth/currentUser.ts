import { createContext, useContext } from 'react';
import { roleAtLeast, type CurrentUser, type OrgUnit } from '../types/api';

export interface Session {
    user: CurrentUser;
    /** Org units the user has any role on, with their effective role. */
    orgUnits: OrgUnit[];
}

export const SessionContext = createContext<Session | null>(null);

/** The signed-in user. Only available inside routes wrapped by RequireAuth. */
export function useCurrentUser(): CurrentUser | null {
    return useContext(SessionContext)?.user ?? null;
}

export interface Access {
    canWriteAnywhere: boolean;
    canManageRoles: boolean;
    /** Whether the user may change records owned by this unit. */
    canWrite: (orgUnitId: number | undefined) => boolean;
    /** Units new records can go in, top of the tree first. */
    writableUnits: OrgUnit[];
    /** Units the user can grant roles on. */
    adminUnits: OrgUnit[];
    orgUnits: OrgUnit[];
}

/**
 * What the signed-in user may do, for hiding or disabling actions. The API enforces the same
 * rules; this only keeps the UI from offering buttons that would be refused.
 */
export function useAccess(): Access {
    const session = useContext(SessionContext);
    const units = session?.orgUnits ?? [];
    const byId = new Map(units.map(u => [u.id, u]));
    const depth = (u: OrgUnit): number => (u.parentId !== null && byId.has(u.parentId) ? 1 + depth(byId.get(u.parentId)!) : 0);
    const topFirst = [...units].sort((a, b) => depth(a) - depth(b) || a.id - b.id);

    return {
        canWriteAnywhere: session?.user.access.canWrite ?? false,
        canManageRoles: session?.user.access.canManageRoles ?? false,
        canWrite: id => id !== undefined && roleAtLeast(byId.get(id)?.myRole, 'Manager'),
        writableUnits: topFirst.filter(u => roleAtLeast(u.myRole, 'Manager')),
        adminUnits: topFirst.filter(u => u.myRole === 'Admin'),
        orgUnits: topFirst,
    };
}

/** Where to go after signing in: only same-site paths, so a crafted link can't bounce the user elsewhere. */
export function safeReturnUrl(value: string | null): string {
    return value && value.startsWith('/') && !value.startsWith('//') ? value : '/home';
}
