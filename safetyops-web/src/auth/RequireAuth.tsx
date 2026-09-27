import { useEffect, useState } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { api } from '../services/api';
import { SessionContext, type Session } from './currentUser';

type AuthState = { status: 'loading' } | { status: 'signedIn'; session: Session } | { status: 'signedOut' };

/** Layout route: renders its child routes only for a signed-in user, otherwise sends them to /login. */
export default function RequireAuth() {
    const location = useLocation();
    const [state, setState] = useState<AuthState>({ status: 'loading' });

    useEffect(() => {
        let cancelled = false;
        (async () => {
            try {
                const user = await api.me();
                // Users with no role at all can sign in but can't list org units.
                const orgUnits = user.access.canRead ? await api.getOrgUnits() : [];
                if (!cancelled) setState({ status: 'signedIn', session: { user, orgUnits } });
            } catch {
                if (!cancelled) setState({ status: 'signedOut' });
            }
        })();
        return () => { cancelled = true; };
    }, []);

    if (state.status === 'loading') return null;
    if (state.status === 'signedOut') {
        const returnUrl = encodeURIComponent(location.pathname + location.search);
        return <Navigate to={`/login?returnUrl=${returnUrl}`} replace />;
    }
    return (
        <SessionContext.Provider value={state.session}>
            <Outlet />
        </SessionContext.Provider>
    );
}
