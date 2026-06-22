import { useNavigate } from 'react-router-dom';
import NavBar from '../components/NavBar';

export default function OMSSShellPage() {
    const navigate = useNavigate();
    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/omss'); }} style={navLink}>
                    Medical Surveillance (OMSS)
                </a>
            } />
            <div style={{ padding: '40px 60px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '24px' }}>Medical Surveillance (OMSS)</h2>
                <div style={{ display: 'flex', gap: '16px' }}>
                    <a href="#" role="link"
                        onClick={e => { e.preventDefault(); navigate('/n/safetyops/omss/create'); }}
                        style={{ background: '#1a2744', color: 'white', textDecoration: 'none', padding: '12px 28px', borderRadius: '4px', fontSize: '15px' }}>
                        Create
                    </a>
                    <a href="#" role="link"
                        onClick={e => { e.preventDefault(); navigate('/n/safetyops/omss/edit'); }}
                        style={{ background: '#1a2744', color: 'white', textDecoration: 'none', padding: '12px 28px', borderRadius: '4px', fontSize: '15px' }}>
                        Edit / Search
                    </a>
                </div>
            </div>
        </div>
    );
}
