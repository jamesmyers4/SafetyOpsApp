import PageLayout from '../components/PageLayout';
import { styles } from '../styles/theme';

export default function MedicalCreatePage() {
    return (
        <PageLayout section={{ label: 'Medical Surveillance', to: '/medical-surveillance' }} variant="shell">
            <h2 style={{ ...styles.heading, marginBottom: '16px' }}>Create Medical Surveillance Record</h2>
            <iframe
                id="create-frame"
                name="create-frame"
                src="/medical-surveillance/create-frame"
                style={{ width: '100%', height: '78vh', border: '1px solid #ddd', borderRadius: '4px', background: 'white' }}
                title="Medical Surveillance Create Frame"
            />
        </PageLayout>
    );
}
