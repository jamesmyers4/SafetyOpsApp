import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../services/api';
import { errorMessage } from '../services/errors';
import type { TrainingClass } from '../types/api';
import ConfirmDelete from '../components/ConfirmDelete';
import LoadStatus from '../components/LoadStatus';
import PageLayout from '../components/PageLayout';
import { styles } from '../styles/theme';

/** Landing page after a class is created from the Training shell. */
export default function ClassDetailPage() {
    const { id } = useParams<{ id: string }>();
    const [cls, setCls] = useState<TrainingClass | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [deleted, setDeleted] = useState(false);

    useEffect(() => {
        let cancelled = false;
        api.getTrainingClass(Number(id))
            .then(result => { if (!cancelled) setCls(result); })
            .catch((e: unknown) => { if (!cancelled) setError(errorMessage(e, 'Failed to load class')); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [id]);

    async function handleDelete() {
        try {
            await api.deleteTrainingClass(Number(id));
            setDeleted(true);
        } catch (e: unknown) {
            setError(errorMessage(e, 'Delete failed'));
        }
    }

    return (
        <PageLayout section={{ label: 'Training', to: '/training' }}>
            <h2 style={{ ...styles.heading, marginBottom: '20px' }}>Training Class Details</h2>

            <div role="status" style={{ ...styles.alertSuccess, marginBottom: '24px', fontWeight: 'bold' }}>
                Class saved successfully
            </div>

            {deleted && <div role="status" style={{ ...styles.alertDanger, marginBottom: '24px' }}>Class deleted successfully.</div>}

            <LoadStatus loading={loading} error={error} />

            {cls && !deleted && (
                <>
                    <div style={styles.card}>
                        <p><strong>Course:</strong> {cls.courseTitle} ({cls.courseId})</p>
                        <p><strong>Date:</strong> {cls.classDate}</p>
                        <p><strong>Location:</strong> {cls.location}</p>
                    </div>
                    <ConfirmDelete label="Delete" prompt="Are you sure?" onConfirm={handleDelete} />
                </>
            )}
        </PageLayout>
    );
}
