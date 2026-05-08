import { useNavigate } from 'react-router-dom';

export default function SplashPage() {
    const navigate = useNavigate();
    return (
        <div style={{ background: '#1a2744', display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', margin: 0 }}>
            <div style={{ background: 'white', padding: '60px', borderRadius: '8px', textAlign: 'center', width: '400px' }}>
                <h1 style={{ color: '#1a2744', marginBottom: '10px' }}>ESAMS</h1>
                <p style={{ color: '#555', marginBottom: '30px' }}>Enterprise Safety and Mission Assurance System</p>
                <button
                    onClick={() => navigate('/login')}
                    style={{ background: '#1a2744', color: 'white', border: 'none', padding: '14px 32px', fontSize: '16px', borderRadius: '4px', cursor: 'pointer' }}
                >
                    Login to ESAMS
                </button>
            </div>
        </div>
    );
}