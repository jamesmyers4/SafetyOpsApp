import { useEffect, useState } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { api, type CurrentUser } from '../services/api';
import { CurrentUserContext } from './currentUser';

type AuthState = { status: 'loading' } | { status: 'signedIn'; user: CurrentUser } | { status: 'signedOut' };

/** Layout route: renders its child routes only for a signed-in user, otherwise sends them to /login. */
export default function RequireAuth() {
    const location = useLocation();
    const [state, setState] = useState<AuthState>({ status: 'loading' });

    useEffect(() => {
        let cancelled = false;
        api.me()
            .then(user => { if (!cancelled) setState({ status: 'signedIn', user }); })
            .catch(() => { if (!cancelled) setState({ status: 'signedOut' }); });
        return () => { cancelled = true; };
    }, []);

    if (state.status === 'loading') return null;
    if (state.status === 'signedOut') {
        const returnUrl = encodeURIComponent(location.pathname + location.search);
        return <Navigate to={`/login?returnUrl=${returnUrl}`} replace />;
    }
    return (
        <CurrentUserContext.Provider value={state.user}>
            <Outlet />
        </CurrentUserContext.Provider>
    );
}
