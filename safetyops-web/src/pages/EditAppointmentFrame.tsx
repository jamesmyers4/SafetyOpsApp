import { useEffect, useState } from 'react';
import { api } from '../services/api';
import { errorMessage } from '../services/errors';
import { postToParent } from '../services/messaging';
import { useAppointmentForm } from '../hooks/useAppointmentForm';
import AppointmentFields from '../components/AppointmentFields';
import LoadStatus from '../components/LoadStatus';
import { styles } from '../styles/theme';
import { useAccess } from '../auth/currentUser';

/** Runs inside the Search / Edit page's iframe; the record id comes from the ?id= query string. */
export default function EditAppointmentFrame() {
    const [appointmentId] = useState(() => Number(new URLSearchParams(window.location.search).get('id')) || null);
    const form = useAppointmentForm();
    const { load } = form;
    const [loading, setLoading] = useState(appointmentId !== null);
    const [loadError, setLoadError] = useState<string | null>(null);
    const [successMsg, setSuccessMsg] = useState('');
    const [errors, setErrors] = useState<string[]>([]);
    const [ownerUnit, setOwnerUnit] = useState<number | undefined>(undefined);
    const access = useAccess();
    const canEdit = access.canWrite(ownerUnit);

    useEffect(() => {
        if (!appointmentId) return;
        let cancelled = false;
        api.getAppointment(appointmentId)
            .then(appointment => { if (!cancelled) { load(appointment); setOwnerUnit(appointment.orgUnitId); } })
            .catch((e: unknown) => { if (!cancelled) setLoadError(errorMessage(e, 'Failed to load record')); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [appointmentId, load]);

    async function handleUpdate() {
        if (!appointmentId) return;
        const errs = form.validate();
        setErrors(errs);
        if (errs.length > 0) return;
        try {
            await api.updateAppointment(appointmentId, form.toInput());
            setSuccessMsg('Record updated successfully');
            postToParent({ type: 'appointmentUpdated', message: 'Record updated successfully' });
        } catch (e: unknown) {
            setErrors([errorMessage(e, 'Update failed')]);
        }
    }

    return (
        <div style={{ ...styles.frame, minHeight: '100%' }}>
            <h3 style={styles.frameHeading}>Edit Medical Surveillance Record</h3>

            <LoadStatus loading={loading} error={loadError} />
            {!loading && !loadError && !canEdit && (
                <div role="status" style={{ ...styles.alertWarning, marginBottom: '12px' }}>You have read-only access to this record.</div>
            )}

            {errors.length > 0 && (
                <div role="alert" style={{ ...styles.errorText, marginBottom: '12px' }}>
                    {errors.map((e, i) => <div key={i}>{e}</div>)}
                </div>
            )}

            {successMsg && <div role="status" style={{ ...styles.alertSuccess, padding: '12px', marginBottom: '12px' }}>{successMsg}</div>}

            <AppointmentFields form={form} workTaskPickerHeight="300px" />

            <div style={{ display: 'flex', gap: '10px', marginTop: '8px' }}>
                <button onClick={handleUpdate} disabled={loading || !!loadError || !canEdit} style={styles.primaryButton}>Update</button>
                <button onClick={() => postToParent({ type: 'appointmentEditCancelled' })} style={styles.secondaryButton}>Cancel</button>
            </div>
        </div>
    );
}
