import { useState, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';

interface NavBarProps {
    extra?: ReactNode;
}

export default function NavBar({ extra }: NavBarProps) {
    const navigate = useNavigate();
    const [modulesOpen, setModulesOpen] = useState(false);
    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <>
            <div style={{ background: '#1a2744', padding: '14px 30px', color: 'white', fontSize: '18px', fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: '30px', position: 'relative', zIndex: 300 }}>
                SAFETYOPS
                <div style={{ position: 'relative' }}>
                    <a
                        href="#"
                        id="aMenuAppDropdown"
                        aria-expanded={modulesOpen}
                        onClick={e => { e.preventDefault(); setModulesOpen(o => !o); }}
                        style={navLink}
                    >
                        Modules
                    </a>
                    {modulesOpen && (
                        <div
                            role="menu"
                            style={{ position: 'absolute', top: '28px', left: 0, background: 'white', borderRadius: '4px', boxShadow: '0 4px 12px rgba(0,0,0,0.2)', minWidth: '260px', zIndex: 400 }}
                        >
                            <a href="#" role="link" onClick={e => { e.preventDefault(); setModulesOpen(false); navigate('/n/safetyops/personnel'); }}
                                style={{ display: 'block', padding: '12px 20px', color: '#1a2744', textDecoration: 'none', fontSize: '14px' }}>
                                Personnel Administration
                            </a>
                            <a href="#" role="link" onClick={e => { e.preventDefault(); setModulesOpen(false); navigate('/n/safetyops/training'); }}
                                style={{ display: 'block', padding: '12px 20px', color: '#1a2744', textDecoration: 'none', fontSize: '14px' }}>
                                Training Administration (TA)
                            </a>
                            <a href="#" role="link" onClick={e => { e.preventDefault(); setModulesOpen(false); navigate('/n/safetyops/omss'); }}
                                style={{ display: 'block', padding: '12px 20px', color: '#1a2744', textDecoration: 'none', fontSize: '14px' }}>
                                Medical Surveillance (OMSS)
                            </a>
                        </div>
                    )}
                </div>
                {extra}
            </div>
            {modulesOpen && (
                <div style={{ position: 'fixed', inset: 0, zIndex: 200 }} onClick={() => setModulesOpen(false)} />
            )}
        </>
    );
}
