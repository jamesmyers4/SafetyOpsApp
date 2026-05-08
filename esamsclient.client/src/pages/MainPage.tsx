import { useNavigate } from 'react-router-dom';

export default function MainPage() {
    const navigate = useNavigate();
    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <div style={{ background: '#1a2744', padding: '14px 30px', color: 'white', fontSize: '18px', fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: '30px' }}>
                ESAMS
                <a onClick={() => navigate('/n/esams/main')} style={{ color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' }}>Modules</a>
                <a onClick={() => navigate('/n/esams/personnel')} style={{ color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' }}>Personnel Administration</a>
            </div>
            <div style={{ padding: '60px', textAlign: 'center' }}>
                <h2 style={{ color: '#1a2744' }}>Welcome to ESAMS</h2>
                <p style={{ color: '#555' }}>Select a module from the navigation bar to get started.</p>
            </div>
        </div>
    );
}