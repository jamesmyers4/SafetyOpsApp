import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { api } from '../services/api';
import { errorMessage } from '../services/errors';
import {
    INCIDENT_CATEGORIES, INCIDENT_SEVERITIES, INCIDENT_STATUSES,
    type Incident, type IncidentInput,
} from '../types/api';
import ConfirmDelete from '../components/ConfirmDelete';
import IncidentForm from '../components/IncidentForm';
import LoadStatus from '../components/LoadStatus';
import PageLayout from '../components/PageLayout';
import { styles } from '../styles/theme';
import { formatIncidentTime } from './incidentFormat';

function toInput({ id: _id, reportedByName: _name, ...input }: Incident): IncidentInput {
    return input;
}

export default function IncidentDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [incident, setIncident] = useState<Incident | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [editing, setEditing] = useState(false);
    const [saveErrors, setSaveErrors] = useState<string[]>([]);
    const [message, setMessage] = useState('');

    useEffect(() => {
        let cancelled = false;
        api.getIncident(Number(id))
            .then(result => { if (!cancelled) setIncident(result); })
            .catch((e: unknown) => { if (!cancelled) setError(errorMessage(e, 'Failed to load incident')); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [id]);

    async function handleSave(input: IncidentInput) {
        try {
            setIncident(await api.updateIncident(Number(id), input));
            setEditing(false);
            setSaveErrors([]);
            setMessage('Incident updated successfully');
        } catch (e: unknown) {
            setSaveErrors([errorMessage(e, 'Update failed')]);
        }
    }

    async function handleDelete() {
        try {
            await api.deleteIncident(Number(id));
            navigate('/incidents');
        } catch (e: unknown) {
            setError(errorMessage(e, 'Delete failed'));
        }
    }

    return (
        <PageLayout section={{ label: 'Incident Reports', to: '/incidents' }}>
            <h2 style={{ ...styles.heading, marginBottom: '20px' }}>Incident Report #{id}</h2>
            <LoadStatus loading={loading} error={error} />
            {message && <div role="status" style={styles.alertSuccess}>{message}</div>}

            {incident && editing && (
                <IncidentForm initial={toInput(incident)} showStatus submitLabel="Save Changes" errors={saveErrors}
                    onSubmit={handleSave} onCancel={() => { setEditing(false); setSaveErrors([]); }} />
            )}

            {incident && !editing && (
                <>
                    <div style={styles.card}>
                        <p><strong>Status:</strong> {INCIDENT_STATUSES[incident.status]}</p>
                        <p><strong>Occurred:</strong> {formatIncidentTime(incident.occurredAt)}</p>
                        <p><strong>Location:</strong> {incident.location}</p>
                        <p><strong>Category:</strong> {INCIDENT_CATEGORIES[incident.category]}</p>
                        <p><strong>Severity:</strong> {INCIDENT_SEVERITIES[incident.severity]}</p>
                        <p><strong>Reported By:</strong> {incident.reportedByName}</p>
                        <p style={{ whiteSpace: 'pre-wrap' }}><strong>Description:</strong> {incident.description}</p>
                    </div>
                    <div style={styles.buttonRow}>
                        <button onClick={() => { setEditing(true); setMessage(''); }} style={styles.primaryButton}>Edit</button>
                        <ConfirmDelete label="Delete" prompt="Delete this incident?" onConfirm={handleDelete} />
                    </div>
                </>
            )}
        </PageLayout>
    );
}
