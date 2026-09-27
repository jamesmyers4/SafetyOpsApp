import { useParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout';
import { colors, styles } from '../styles/theme';

export default function IncidentDetailPage() {
    const { id } = useParams<{ id: string }>();
    return (
        <PageLayout section={{ label: 'Incident Reports', to: '/incidents' }}>
            <h2 style={{ ...styles.heading, marginBottom: '20px' }}>Incident Report #{id}</h2>
            <p style={{ color: colors.muted }}>Incident detail — full implementation pending.</p>
        </PageLayout>
    );
}
