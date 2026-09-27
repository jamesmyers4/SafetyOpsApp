import NavBar from '../components/NavBar';
import { colors, styles } from '../styles/theme';

export default function MainPage() {
    return (
        <div style={styles.page}>
            <NavBar />
            <div style={{ padding: '60px', textAlign: 'center' }}>
                <h2 style={{ color: colors.navy }}>Welcome to SAFETYOPS</h2>
                <p style={{ color: colors.muted }}>Select a module from the navigation bar to get started.</p>
            </div>
        </div>
    );
}
