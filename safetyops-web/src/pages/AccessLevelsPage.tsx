import PageLayout from '../components/PageLayout';
import { colors, styles } from '../styles/theme';

export default function AccessLevelsPage() {
    return (
        <PageLayout section={{ label: 'Personnel', to: '/personnel' }}>
            <h2 style={styles.heading}>Access Levels</h2>
            <p style={{ color: colors.muted }}>Access level management coming soon.</p>
        </PageLayout>
    );
}
