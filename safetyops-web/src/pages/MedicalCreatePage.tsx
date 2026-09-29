import { useNavigate } from 'react-router-dom';
import NavBar from '../components/NavBar';

export default function MedicalCreatePage() {
    const navigate = useNavigate();
    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/medical-surveillance'); }} style={navLink}>
                    Medical Surveillance
                </a>
            } />
            <div style={{ padding: '20px 30px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '16px' }}>Create Medical Surveillance Record</h2>
                <iframe
                    id="create-frame"
                    name="create-frame"
                    src="/medical-surveillance/create-frame"
                    style={{ width: '100%', height: '78vh', border: '1px solid #ddd', borderRadius: '4px', background: 'white' }}
                    title="Medical Surveillance Create Frame"
                />
            </div>
        </div>
    );
}
