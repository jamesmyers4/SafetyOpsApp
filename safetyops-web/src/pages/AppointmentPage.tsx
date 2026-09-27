import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../services/api';
import { errorMessage } from '../services/errors';
import type { Appointment } from '../types/api';
import ConfirmDelete from '../components/ConfirmDelete';
import LoadStatus from '../components/LoadStatus';
import PageLayout from '../components/PageLayout';
import { styles } from '../styles/theme';

/** Landing page after an appointment is created. */
export default function AppointmentPage() {
    const { id } = useParams<{ id: string }>();
    const [appt, setAppt] = useState<Appointment | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [deleted, setDeleted] = useState(false);

    useEffect(() => {
        let cancelled = false;
        api.getAppointment(Number(id))
            .then(result => { if (!cancelled) setAppt(result); })
            .catch((e: unknown) => { if (!cancelled) setError(errorMessage(e, 'Failed to load appointment')); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [id]);

    async function handleDelete() {
        try {
            await api.deleteAppointment(Number(id));
            setDeleted(true);
        } catch (e: unknown) {
            setError(errorMessage(e, 'Delete failed'));
        }
    }

    return (
        <PageLayout section={{ label: 'Medical Surveillance', to: '/medical-surveillance' }}>
            <h2 style={{ ...styles.heading, marginBottom: '20px' }}>Appointment Details</h2>

            <LoadStatus loading={loading} error={error} />

            {deleted && <div role="status" style={{ ...styles.alertDanger, marginBottom: '24px' }}>Appointment deleted successfully.</div>}

            {appt && !deleted && (
                <>
                    <div style={styles.card}>
                        <p><strong>ID:</strong> {appt.id}</p>
                        <p><strong>Date:</strong> {appt.date}</p>
                        <p><strong>Person Evaluated:</strong> {appt.personName}</p>
                        {appt.stressors.length > 0 && (
                            <>
                                <p><strong>Stressors:</strong></p>
                                <ul>
                                    {appt.stressors.map(s => <li key={s.stressorId}>{s.stressorName} — {s.examType}</li>)}
                                </ul>
                            </>
                        )}
                    </div>
                    <ConfirmDelete label="Delete Appointment" prompt="Confirm deletion?" onConfirm={handleDelete} />
                </>
            )}
        </PageLayout>
    );
}
