import { useNavigate } from 'react-router-dom';
import NavBar from '../components/NavBar';

export default function OMSSCreatePage() {
    const navigate = useNavigate();
    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/omss'); }} style={navLink}>
                    Medical Surveillance (OMSS)
                </a>
            } />
            <div style={{ padding: '20px 30px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '16px' }}>Create Medical Surveillance Record</h2>
                <iframe
                    id="frameCreate"
                    name="frameCreate"
                    src="/n/safetyops/omss/create-frame"
                    style={{ width: '100%', height: '78vh', border: '1px solid #ddd', borderRadius: '4px', background: 'white' }}
                    title="OMSS Create Frame"
                />
            </div>
        </div>
    );
}
