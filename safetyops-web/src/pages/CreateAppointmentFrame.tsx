import { useState } from 'react';
import { api } from '../services/api';
import { errorMessage } from '../services/errors';
import { useAppointmentForm } from '../hooks/useAppointmentForm';
import AppointmentFields from '../components/AppointmentFields';
import { styles } from '../styles/theme';

/** Runs inside the Create Medical Surveillance Record page's iframe. */
export default function CreateAppointmentFrame() {
    const form = useAppointmentForm();
    const [errors, setErrors] = useState<string[]>([]);

    async function handleCreate() {
        const errs = form.validate();
        setErrors(errs);
        if (errs.length > 0) return;
        try {
            const created = await api.createAppointment(form.toInput());
            window.parent.location.href = `/medical-surveillance/appointments/${created.id}`;
        } catch (e: unknown) {
            setErrors([errorMessage(e, 'Create failed')]);
        }
    }

    return (
        <div style={styles.frame}>
            <h3 style={styles.frameHeading}>Create Medical Surveillance Record</h3>

            {errors.length > 0 && (
                <div role="alert" style={{ ...styles.errorText, marginBottom: '12px' }}>
                    {errors.map((e, i) => <div key={i}>{e}</div>)}
                </div>
            )}

            <AppointmentFields form={form} workTaskPickerHeight="350px" />

            <button onClick={handleCreate} style={styles.primaryButton}>Create</button>
        </div>
    );
}
