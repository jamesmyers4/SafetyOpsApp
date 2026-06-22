import { useNavigate, useParams } from 'react-router-dom';
import NavBar from '../components/NavBar';

export default function FirIncidentPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/fir'); }} style={navLink}>
                    Fire Incident Reporting
                </a>
            } />
            <div style={{ padding: '40px 60px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '20px' }}>Fire Incident Report #{id}</h2>
                <p style={{ color: '#555' }}>Incident detail — full implementation pending.</p>
            </div>
        </div>
    );
}
