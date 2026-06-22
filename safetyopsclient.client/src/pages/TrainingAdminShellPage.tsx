import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import NavBar from '../components/NavBar';

interface PendingData {
    id?: number;
    courseTitle: string;
    courseId: string;
    classDate: string;
    location: string;
    isUpdate: boolean;
}

export default function TrainingAdminShellPage() {
    const navigate = useNavigate();
    const [iframeSrc, setIframeSrc] = useState('/n/safetyops/training/edit-frame');
    const [pending, setPending] = useState<PendingData | null>(null);
    const [showSave, setShowSave] = useState(false);
    const [successMsg, setSuccessMsg] = useState('');

    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    const handleMessage = useCallback((event: MessageEvent) => {
        const msg = event.data ?? {};
        if (msg.type === 'trainingReadyToSave') {
            setPending({ ...msg.data, isUpdate: !!msg.isUpdate });
            setShowSave(true);
            setSuccessMsg('');
        } else if (msg.type === 'trainingFormReset') {
            setPending(null);
            setShowSave(false);
            setSuccessMsg('');
        } else if (msg.type === 'trainingGoToExisting' && msg.id) {
            navigate(`/n/safetyops/training/class/${msg.id}`);
        }
    }, [navigate]);

    useEffect(() => {
        window.addEventListener('message', handleMessage);
        return () => window.removeEventListener('message', handleMessage);
    }, [handleMessage]);

    function switchToCreate() {
        setPending(null);
        setShowSave(false);
        setSuccessMsg('');
        setIframeSrc('/n/safetyops/training/create-frame?t=' + Date.now());
    }

    function switchToSearch() {
        setPending(null);
        setShowSave(false);
        setSuccessMsg('');
        setIframeSrc('/n/safetyops/training/edit-frame?t=' + Date.now());
    }

    async function handleSave() {
        if (!pending) {
            setShowSave(false);
            return;
        }
        try {
            if (pending.isUpdate && pending.id) {
                await api.updateTrainingClass(pending.id, {
                    courseTitle: pending.courseTitle,
                    courseId: pending.courseId,
                    classDate: pending.classDate,
                    location: pending.location,
                });
                setSuccessMsg('Class saved successfully');
                setShowSave(false);
                setPending(null);
            } else {
                const result = await api.createTrainingClass({
                    courseTitle: pending.courseTitle,
                    courseId: pending.courseId,
                    classDate: pending.classDate,
                    location: pending.location,
                });
                navigate(`/n/safetyops/training/class/${result.id}`);
            }
        } catch (e) {
            console.error(e);
        }
    }

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/training'); }} style={navLink}>
                    Training Administration (TA)
                </a>
            } />
            <div style={{ padding: '20px 30px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '16px' }}>Training Administration (TA)</h2>
                <div style={{ marginBottom: '16px', display: 'flex', gap: '12px', alignItems: 'center' }}>
                    <a href="#" role="link"
                        onClick={e => { e.preventDefault(); switchToCreate(); }}
                        style={{ color: '#1a2744', fontWeight: 'bold', textDecoration: 'underline', cursor: 'pointer', fontSize: '15px' }}>
                        Create Class
                    </a>
                    <span style={{ color: '#ccc' }}>|</span>
                    <a href="#" role="link"
                        onClick={e => { e.preventDefault(); switchToSearch(); }}
                        style={{ color: '#1a2744', textDecoration: 'underline', cursor: 'pointer', fontSize: '15px' }}>
                        Search / Edit Classes
                    </a>
                </div>

                {successMsg && (
                    <div style={{ background: '#d4edda', color: '#155724', padding: '12px 20px', borderRadius: '4px', marginBottom: '12px', fontSize: '14px' }}>
                        {successMsg}
                    </div>
                )}

                {showSave && (
                    <div style={{ marginBottom: '12px' }}>
                        <button onClick={handleSave}
                            style={{ background: '#1a2744', color: 'white', border: 'none', padding: '10px 28px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer' }}>
                            Save
                        </button>
                    </div>
                )}

                <iframe
                    src={iframeSrc}
                    style={{ width: '100%', height: '72vh', border: '1px solid #ddd', borderRadius: '4px', background: 'white' }}
                    title="Training Admin Frame"
                />
            </div>
        </div>
    );
}
