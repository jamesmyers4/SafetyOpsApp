import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import { errorMessage } from '../services/errors';
import { useAppMessages } from '../services/messaging';
import PageLayout from '../components/PageLayout';
import type { ClassDraft } from '../types/messages';
import { colors, styles } from '../styles/theme';
import { useAccess } from '../auth/currentUser';

interface PendingSave extends ClassDraft {
    isUpdate: boolean;
}

/**
 * Hosts the create and search/edit class frames. A frame validates its form and posts the
 * draft here; the shell's Save button is what actually writes to the API.
 */
export default function TrainingShellPage() {
    const navigate = useNavigate();
    const [iframeSrc, setIframeSrc] = useState('/training/edit-frame');
    const [pending, setPending] = useState<PendingSave | null>(null);
    const [successMsg, setSuccessMsg] = useState('');
    const [errorMsg, setErrorMsg] = useState('');
    const access = useAccess();

    useAppMessages(message => {
        if (message.type === 'trainingReadyToSave') {
            setPending({ ...message.data, isUpdate: !!message.isUpdate });
            setSuccessMsg('');
        } else if (message.type === 'trainingFormReset') {
            setPending(null);
            setSuccessMsg('');
        } else if (message.type === 'trainingGoToExisting') {
            navigate(`/training/classes/${message.id}`);
        }
    });

    function switchFrame(path: string) {
        setPending(null);
        setSuccessMsg('');
        setErrorMsg('');
        setIframeSrc(`${path}?t=${Date.now()}`);
    }

    async function handleSave() {
        if (!pending) return;
        setErrorMsg('');
        const input = { courseId: pending.courseId, classDate: pending.classDate, location: pending.location, orgUnitId: pending.orgUnitId };
        try {
            if (pending.isUpdate && pending.id) {
                await api.updateTrainingClass(pending.id, input);
                setSuccessMsg('Class saved successfully');
                setPending(null);
            } else {
                const created = await api.createTrainingClass(input);
                navigate(`/training/classes/${created.id}`);
            }
        } catch (e: unknown) {
            setErrorMsg(errorMessage(e, 'Save failed'));
        }
    }

    const tab = { color: colors.navy, textDecoration: 'underline', cursor: 'pointer', fontSize: '15px' };

    return (
        <PageLayout section={{ label: 'Training', to: '/training' }} variant="shell">
            <h2 style={{ ...styles.heading, marginBottom: '16px' }}>Training</h2>
            <div style={{ marginBottom: '16px', display: 'flex', gap: '12px', alignItems: 'center' }}>
                {access.canWriteAnywhere && (
                    <>
                        <a href="#" role="link" onClick={e => { e.preventDefault(); switchFrame('/training/create-frame'); }}
                            style={{ ...tab, fontWeight: 'bold' }}>
                            Create Class
                        </a>
                        <span style={{ color: colors.border }}>|</span>
                    </>
                )}
                <a href="#" role="link" onClick={e => { e.preventDefault(); switchFrame('/training/edit-frame'); }} style={tab}>
                    Search / Edit Classes
                </a>
            </div>

            {errorMsg && <div role="alert" style={{ ...styles.alertDanger, marginBottom: '12px' }}>{errorMsg}</div>}
            {successMsg && <div role="status" style={{ ...styles.alertSuccess, marginBottom: '12px' }}>{successMsg}</div>}

            {pending && (
                <div style={{ marginBottom: '12px' }}>
                    <button onClick={handleSave} style={styles.primaryButton}>Save</button>
                </div>
            )}

            <iframe
                src={iframeSrc}
                style={{ width: '100%', height: '72vh', border: '1px solid #ddd', borderRadius: '4px', background: 'white' }}
                title="Training Frame"
            />
        </PageLayout>
    );
}
