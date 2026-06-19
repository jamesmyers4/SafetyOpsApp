import { useNavigate } from 'react-router-dom';

export default function SuccessPage() {
    const navigate = useNavigate();
    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <div style={{ background: '#1a2744', padding: '14px 30px', color: 'white', fontSize: '18px', fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: '30px' }}>
                SAFETYOPS
                <a onClick={() => navigate('/n/safetyops/main')} style={{ color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' }}>Modules</a>
                <a onClick={() => navigate('/n/safetyops/personnel')} style={{ color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' }}>Personnel Administration</a>
            </div>
            <div style={{ padding: '60px', textAlign: 'center' }}>
                <h2 style={{ color: '#2a7a2a', marginBottom: '20px' }}>User Successfully Added</h2>
                <p style={{ color: '#555', marginBottom: '40px' }}>The new user has been created in the system.</p>
                <button
                    onClick={() => navigate('/n/safetyops/personnel/create')}
                    style={{ background: '#1a2744', color: 'white', border: 'none', padding: '12px 28px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer', margin: '8px' }}
                >
                    Add Another User
                </button>
                <button
                    onClick={() => navigate('/n/safetyops/personnel')}
                    style={{ background: '#1a2744', color: 'white', border: 'none', padding: '12px 28px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer', margin: '8px' }}
                >
                    Return to PA Home
                </button>
            </div>
        </div>
    );
}