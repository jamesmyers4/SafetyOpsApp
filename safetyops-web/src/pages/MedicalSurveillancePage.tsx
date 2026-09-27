import NavLink from '../components/NavLink';
import PageLayout from '../components/PageLayout';
import { styles } from '../styles/theme';
import { useAccess } from '../auth/currentUser';

const tile = { ...styles.buttonLink, padding: '12px 28px' };

export default function MedicalSurveillancePage() {
    const access = useAccess();
    return (
        <PageLayout section={{ label: 'Medical Surveillance', to: '/medical-surveillance' }}>
            <h2 style={{ ...styles.heading, marginBottom: '24px' }}>Medical Surveillance</h2>
            <div style={{ display: 'flex', gap: '16px' }}>
                {access.canWriteAnywhere && <NavLink to="/medical-surveillance/create" role="link" style={tile}>Create</NavLink>}
                <NavLink to="/medical-surveillance/edit" role="link" style={tile}>Edit / Search</NavLink>
            </div>
        </PageLayout>
    );
}
