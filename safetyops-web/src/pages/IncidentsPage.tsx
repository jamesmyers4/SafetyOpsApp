import { useNavigate } from 'react-router-dom';
import NavBar from '../components/NavBar';

export default function IncidentsPage() {
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
                <h2 style={{ color: '#1a2744', marginBottom: '20px' }}>Incident Reports</h2>
                <div style={{ marginBottom: '20px' }}>
                    <a href="#" role="link"
                        onClick={e => { e.preventDefault(); navigate('/incidents/new'); }}
                        style={{ background: '#1a2744', color: 'white', textDecoration: 'none', padding: '10px 24px', borderRadius: '4px', fontSize: '15px' }}>
                        Create Incident
                    </a>
                </div>
                <p style={{ color: '#555' }}>Select an option above to get started.</p>
            </div>
        </div>
    );
}
