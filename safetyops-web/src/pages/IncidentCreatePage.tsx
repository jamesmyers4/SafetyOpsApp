import { useNavigate } from 'react-router-dom';
import NavBar from '../components/NavBar';

export default function IncidentCreatePage() {
    const navigate = useNavigate();
    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/incidents'); }} style={navLink}>
                    Incident Reports
                </a>
            } />
            <div style={{ padding: '40px 60px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '20px' }}>Report an Incident</h2>
                <p style={{ color: '#555' }}>The incident report form is coming soon.</p>
            </div>
        </div>
    );
}
