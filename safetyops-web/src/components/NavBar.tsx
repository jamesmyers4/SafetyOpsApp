import { useState, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import { useCurrentUser } from '../auth/currentUser';
import { colors, styles } from '../styles/theme';

interface NavBarProps {
    extra?: ReactNode;
}

export default function NavBar({ extra }: NavBarProps) {
    const navigate = useNavigate();
    const [modulesOpen, setModulesOpen] = useState(false);
    const user = useCurrentUser();

    async function signOut() {
        try {
            await api.logout();
        } finally {
            navigate('/login');
        }
    }

    return (
        <>
            <div style={{ background: colors.navy, padding: '14px 30px', color: 'white', fontSize: '18px', fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: '30px', position: 'relative', zIndex: 300 }}>
                SAFETYOPS
                <div style={{ position: 'relative' }}>
                    <a
                        href="#"
                        id="modules-menu"
                        aria-expanded={modulesOpen}
                        onClick={e => { e.preventDefault(); setModulesOpen(o => !o); }}
                        style={styles.navLink}
                    >
                        Modules
                    </a>
                    {modulesOpen && (
                        <div
                            role="menu"
                            style={{ position: 'absolute', top: '28px', left: 0, background: 'white', borderRadius: '4px', boxShadow: '0 4px 12px rgba(0,0,0,0.2)', minWidth: '260px', zIndex: 400 }}
                        >
                            <a href="#" role="link" onClick={e => { e.preventDefault(); setModulesOpen(false); navigate('/personnel'); }}
                                style={{ display: 'block', padding: '12px 20px', color: colors.navy, textDecoration: 'none', fontSize: '14px' }}>
                                Personnel
                            </a>
                            <a href="#" role="link" onClick={e => { e.preventDefault(); setModulesOpen(false); navigate('/training'); }}
                                style={{ display: 'block', padding: '12px 20px', color: colors.navy, textDecoration: 'none', fontSize: '14px' }}>
                                Training
                            </a>
                            <a href="#" role="link" onClick={e => { e.preventDefault(); setModulesOpen(false); navigate('/medical-surveillance'); }}
                                style={{ display: 'block', padding: '12px 20px', color: colors.navy, textDecoration: 'none', fontSize: '14px' }}>
                                Medical Surveillance
                            </a>
                            <a href="#" role="link" onClick={e => { e.preventDefault(); setModulesOpen(false); navigate('/incidents'); }}
                                style={{ display: 'block', padding: '12px 20px', color: colors.navy, textDecoration: 'none', fontSize: '14px' }}>
                                Incident Reports
                            </a>
                        </div>
                    )}
                </div>
                {extra}
                {user && (
                    <div style={{ marginLeft: 'auto', display: 'flex', gap: '16px', alignItems: 'center', fontSize: '14px', fontWeight: 'normal' }}>
                        <span aria-label="Signed in user" title={user.access.grants.map(g => g.role + " on " + g.orgUnitName).join(", ") || "No roles"}>{user.displayName}</span>
                        <a href="#" onClick={e => { e.preventDefault(); signOut(); }} style={styles.navLink}>Sign out</a>
                    </div>
                )}
            </div>
            {modulesOpen && (
                <div style={{ position: 'fixed', inset: 0, zIndex: 200 }} onClick={() => setModulesOpen(false)} />
            )}
        </>
    );
}
