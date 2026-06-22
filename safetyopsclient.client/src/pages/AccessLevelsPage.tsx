import { useNavigate } from 'react-router-dom';
import NavBar from '../components/NavBar';

export default function AccessLevelsPage() {
    const navigate = useNavigate();
    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/personnel'); }} style={navLink}>
                    Personnel Administration
                </a>
            } />
            <div style={{ padding: '40px 60px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '30px' }}>Access Levels</h2>
                <p style={{ color: '#555' }}>Access level management coming soon.</p>
            </div>
        </div>
    );
}
