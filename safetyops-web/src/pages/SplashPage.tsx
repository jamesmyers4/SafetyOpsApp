import { useNavigate } from 'react-router-dom';
import { colors, styles } from '../styles/theme';

export default function SplashPage() {
    const navigate = useNavigate();
    return (
        <div style={{ background: colors.navy, display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', margin: 0 }}>
            <div style={{ background: 'white', padding: '60px', borderRadius: '8px', textAlign: 'center', width: '400px' }}>
                <h1 style={{ color: colors.navy, marginBottom: '10px' }}>SAFETYOPS</h1>
                <p style={{ color: colors.muted, marginBottom: '30px' }}>Workplace safety, training, and medical surveillance tracking</p>
                <button onClick={() => navigate('/login')} style={{ ...styles.primaryButton, padding: '14px 32px', fontSize: '16px' }}>
                    Sign in
                </button>
            </div>
        </div>
    );
}
