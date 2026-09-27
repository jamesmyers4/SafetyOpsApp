import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import { errorMessage } from '../services/errors';
import type { IncidentInput } from '../types/api';
import IncidentForm from '../components/IncidentForm';
import PageLayout from '../components/PageLayout';
import { styles } from '../styles/theme';
import { nowForInput } from './incidentFormat';

export default function IncidentCreatePage() {
    const navigate = useNavigate();
    const [errors, setErrors] = useState<string[]>([]);
    const [initial] = useState<IncidentInput>(() => ({
        occurredAt: nowForInput(),
        location: '',
        category: 'NearMiss',
        severity: 'Low',
        description: '',
        reportedById: 0,
        status: 'Open',
    }));

    async function handleSubmit(input: IncidentInput) {
        try {
            const created = await api.createIncident(input);
            navigate(`/incidents/${created.id}`);
        } catch (e: unknown) {
            setErrors([errorMessage(e, 'Could not save the incident')]);
        }
    }

    return (
        <PageLayout section={{ label: 'Incident Reports', to: '/incidents' }}>
            <h2 style={{ ...styles.heading, marginBottom: '20px' }}>Report an Incident</h2>
            <IncidentForm initial={initial} showStatus={false} submitLabel="Submit Report" errors={errors}
                onSubmit={handleSubmit} onCancel={() => navigate('/incidents')} />
        </PageLayout>
    );
}
