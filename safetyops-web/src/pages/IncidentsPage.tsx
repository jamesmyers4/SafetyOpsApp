import NavLink from '../components/NavLink';
import PageLayout from '../components/PageLayout';
import { colors, styles } from '../styles/theme';

export default function IncidentsPage() {
    return (
        <PageLayout section={{ label: 'Incident Reports', to: '/incidents' }}>
            <h2 style={{ ...styles.heading, marginBottom: '20px' }}>Incident Reports</h2>
            <div style={styles.field}>
                <NavLink to="/incidents/new" role="link" style={styles.buttonLink}>Create Incident</NavLink>
            </div>
            <p style={{ color: colors.muted }}>Select an option above to get started.</p>
        </PageLayout>
    );
}
