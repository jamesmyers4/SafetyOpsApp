import PageLayout from '../components/PageLayout';
import { colors, styles } from '../styles/theme';

export default function IncidentCreatePage() {
    return (
        <PageLayout section={{ label: 'Incident Reports', to: '/incidents' }}>
            <h2 style={{ ...styles.heading, marginBottom: '20px' }}>Report an Incident</h2>
            <p style={{ color: colors.muted }}>The incident report form is coming soon.</p>
        </PageLayout>
    );
}
