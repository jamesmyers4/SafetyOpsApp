import { useNavigate } from 'react-router-dom';
import NavBar from '../components/NavBar';
import NavLink from '../components/NavLink';
import { colors, styles } from '../styles/theme';

export default function SuccessPage() {
    const navigate = useNavigate();
    return (
        <div style={styles.page}>
            <NavBar extra={<NavLink to="/personnel">Personnel</NavLink>} />
            <div style={{ padding: '60px', textAlign: 'center' }}>
                <h2 style={{ color: '#2a7a2a', marginBottom: '20px' }}>User Successfully Added</h2>
                <p style={{ color: colors.muted, marginBottom: '40px' }}>The new user has been created in the system.</p>
                <button onClick={() => navigate('/personnel/create')} style={{ ...styles.primaryButton, margin: '8px' }}>
                    Add Another User
                </button>
                <button onClick={() => navigate('/personnel')} style={{ ...styles.primaryButton, margin: '8px' }}>
                    Return to Personnel
                </button>
            </div>
        </div>
    );
}
