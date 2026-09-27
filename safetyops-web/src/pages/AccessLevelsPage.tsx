import { useEffect, useState, type FormEvent } from 'react';
import { api } from '../services/api';
import { errorMessage } from '../services/errors';
import { useAccess, useCurrentUser } from '../auth/currentUser';
import { ROLES, type OrgUnit, type Role, type RoleAssignment, type UserOption } from '../types/api';
import LoadStatus from '../components/LoadStatus';
import OrgUnitSelect from '../components/OrgUnitSelect';
import PageLayout from '../components/PageLayout';
import { colors, styles } from '../styles/theme';

const ROLE_HELP: Record<Role, string> = {
    Viewer: 'read-only',
    Manager: 'create, edit, and delete records',
    Admin: 'Manager, plus grant and revoke roles',
};

/** Access Levels: the org units you can see, and (for admins) who holds which role where. */
export default function AccessLevelsPage() {
    const access = useAccess();
    const user = useCurrentUser();

    return (
        <PageLayout section={{ label: 'Personnel', to: '/personnel' }}>
            <h2 style={styles.heading}>Access Levels</h2>
            <p style={{ color: colors.muted, maxWidth: '760px' }}>
                A role applies to the organization unit it is granted on and every unit below it.
                Viewers can read, Managers can also create, edit, and delete, and Admins can also grant and revoke roles.
            </p>

            <h3 style={{ color: colors.navy }}>Your organization units</h3>
            {access.orgUnits.length === 0
                ? <p style={styles.emptyText}>You don't have a role on any organization unit yet. Ask an administrator for access.</p>
                : <OrgTree units={access.orgUnits} />}
            {user && user.access.grants.length > 0 && (
                <p style={{ color: colors.muted, fontSize: '14px' }}>
                    Granted to you: {user.access.grants.map(g => `${g.role} on ${g.orgUnitName}`).join('; ')}.
                </p>
            )}

            <h3 style={{ color: colors.navy, marginTop: '32px' }}>Role assignments</h3>
            {access.canManageRoles
                ? <RoleAssignments adminUnits={access.adminUnits} currentUserId={user?.id} />
                : <p style={styles.emptyText}>Only administrators can view and manage role assignments.</p>}
        </PageLayout>
    );
}

function OrgTree({ units }: { units: OrgUnit[] }) {
    const ids = new Set(units.map(u => u.id));
    const children = (parentId: number | null) =>
        units.filter(u => (parentId === null ? u.parentId === null || !ids.has(u.parentId) : u.parentId === parentId));

    const render = (list: OrgUnit[]) => (
        <ul style={{ listStyle: 'none', paddingLeft: '20px', margin: '4px 0' }}>
            {list.map(u => (
                <li key={u.id} data-org-unit={u.code}>
                    <span style={{ fontWeight: 'bold', color: colors.navy }}>{u.name}</span>
                    <span style={{ color: colors.muted, fontSize: '13px' }}> ({u.code}) · you: {u.myRole}</span>
                    {children(u.id).length > 0 && render(children(u.id))}
                </li>
            ))}
        </ul>
    );
    return <div role="tree" aria-label="Organization units" style={{ ...styles.card, padding: '12px 16px' }}>{render(children(null))}</div>;
}

function RoleAssignments({ adminUnits, currentUserId }: { adminUnits: OrgUnit[]; currentUserId?: number }) {
    const [assignments, setAssignments] = useState<RoleAssignment[] | null>(null);
    const [users, setUsers] = useState<UserOption[]>([]);
    const [loadError, setLoadError] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [message, setMessage] = useState('');
    const [grant, setGrant] = useState<{ userId: number; orgUnitId: number | undefined; role: Role }>(
        { userId: 0, orgUnitId: adminUnits[0]?.id, role: 'Viewer' });

    const [reloadKey, setReloadKey] = useState(0);
    const reload = () => setReloadKey(k => k + 1);

    useEffect(() => {
        let cancelled = false;
        Promise.all([api.listRoleAssignments(), api.listAccessUsers()])
            .then(([list, userList]) => { if (!cancelled) { setAssignments(list); setUsers(userList); setLoadError(null); } })
            .catch((e: unknown) => { if (!cancelled) setLoadError(errorMessage(e, 'Failed to load role assignments')); });
        return () => { cancelled = true; };
    }, [reloadKey]);

    async function handleGrant(e: FormEvent) {
        e.preventDefault();
        setMessage('');
        if (!grant.userId || !grant.orgUnitId) {
            setError('Choose a user and an organization unit.');
            return;
        }
        try {
            const created = await api.grantRole({ userId: grant.userId, orgUnitId: grant.orgUnitId, role: grant.role });
            setError(null);
            setMessage(`Granted ${created.role} on ${created.orgUnitName} to ${created.displayName}.`);
            reload();
        } catch (err: unknown) {
            setError(errorMessage(err, 'Could not grant the role'));
        }
    }

    async function handleRevoke(a: RoleAssignment) {
        if (!window.confirm(`Revoke ${a.role} on ${a.orgUnitName} from ${a.displayName}?`)) return;
        setMessage('');
        try {
            await api.revokeRole(a.id);
            setError(null);
            setMessage(`Revoked ${a.role} on ${a.orgUnitName} from ${a.displayName}.`);
            reload();
        } catch (err: unknown) {
            setError(errorMessage(err, 'Could not revoke the role'));
        }
    }

    const select = { ...styles.input, width: '240px' };

    return (
        <>
            <LoadStatus loading={!assignments && !loadError} error={loadError} />
            {error && <div role="alert" style={styles.errorText}>{error}</div>}
            {message && <div role="status" style={styles.alertSuccess}>{message}</div>}

            <form onSubmit={handleGrant} style={{ ...styles.card, display: 'flex', gap: '16px', alignItems: 'flex-end', flexWrap: 'wrap' }}>
                <div>
                    <label htmlFor="grant-user" style={styles.label}>User</label>
                    <select id="grant-user" value={grant.userId || ''} onChange={e => setGrant(g => ({ ...g, userId: Number(e.target.value) }))} style={select}>
                        <option value="">Select a user</option>
                        {users.map(u => <option key={u.id} value={u.id}>{u.displayName} ({u.userName})</option>)}
                    </select>
                </div>
                <div>
                    <label htmlFor="grant-org-unit" style={styles.label}>Organization Unit</label>
                    <OrgUnitSelect id="grant-org-unit" units={adminUnits} value={grant.orgUnitId}
                        onChange={orgUnitId => setGrant(g => ({ ...g, orgUnitId }))} style={select} />
                </div>
                <div>
                    <label htmlFor="grant-role" style={styles.label}>Role</label>
                    <select id="grant-role" value={grant.role} onChange={e => setGrant(g => ({ ...g, role: e.target.value as Role }))} style={select}>
                        {Object.keys(ROLES).map(r => <option key={r} value={r}>{r} — {ROLE_HELP[r as Role]}</option>)}
                    </select>
                </div>
                <button type="submit" style={styles.primaryButton}>Grant Role</button>
            </form>

            {assignments && assignments.length === 0 && <p style={styles.emptyText}>No role assignments in your organization units.</p>}
            {assignments && assignments.length > 0 && (
                <table style={styles.cardTable}>
                    <thead>
                        <tr>
                            <th style={styles.th}>User</th>
                            <th style={styles.th}>Organization Unit</th>
                            <th style={styles.th}>Role</th>
                            <th style={styles.th}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {assignments.map(a => (
                            <tr key={a.id}>
                                <td style={styles.td}>{a.displayName} <span style={{ color: colors.muted }}>({a.userName})</span></td>
                                <td style={styles.td}>{a.orgUnitName}</td>
                                <td style={styles.td}>{a.role}</td>
                                <td style={styles.td}>
                                    {a.userId === currentUserId
                                        ? <span style={{ color: colors.muted, fontSize: '13px' }}>(you)</span>
                                        : <button onClick={() => handleRevoke(a)} style={styles.smallDangerButton}>Revoke</button>}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </>
    );
}
