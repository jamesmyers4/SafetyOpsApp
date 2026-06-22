import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import NavBar from '../components/NavBar';

interface TrainingClass {
    id: number;
    courseTitle: string;
    courseId: string;
    classDate: string;
    location: string;
}

export default function ClassDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [cls, setCls] = useState<TrainingClass | null>(null);
    const [showConfirm, setShowConfirm] = useState(false);
    const [deleted, setDeleted] = useState(false);

    useEffect(() => {
        if (id) {
            api.getTrainingClass(Number(id)).then(setCls).catch(console.error);
        }
    }, [id]);

    async function handleDelete() {
        try {
            await api.deleteTrainingClass(Number(id));
            setDeleted(true);
            setShowConfirm(false);
        } catch (e) {
            console.error(e);
        }
    }

    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/training'); }} style={navLink}>
                    Training Administration (TA)
                </a>
            } />
            <div style={{ padding: '40px 60px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '20px' }}>Training Class Details</h2>

                <div style={{ background: '#d4edda', color: '#155724', padding: '12px 20px', borderRadius: '4px', marginBottom: '24px', fontSize: '14px', fontWeight: 'bold' }}>
                    Class saved successfully
                </div>

                {deleted && (
                    <div style={{ background: '#f8d7da', color: '#721c24', padding: '12px 20px', borderRadius: '4px', marginBottom: '24px', fontSize: '14px' }}>
                        Class deleted successfully.
                    </div>
                )}

                {cls && !deleted && (
                    <div style={{ background: 'white', padding: '24px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)', marginBottom: '24px' }}>
                        <p><strong>Course:</strong> {cls.courseTitle} ({cls.courseId})</p>
                        <p><strong>Date:</strong> {cls.classDate}</p>
                        <p><strong>Location:</strong> {cls.location}</p>
                    </div>
                )}

                {!deleted && (
                    <div style={{ display: 'flex', gap: '12px' }}>
                        {!showConfirm && (
                            <button onClick={() => setShowConfirm(true)}
                                style={{ background: '#cc0000', color: 'white', border: 'none', padding: '10px 24px', borderRadius: '4px', cursor: 'pointer', fontSize: '15px' }}>
                                Delete
                            </button>
                        )}
                        {showConfirm && (
                            <>
                                <span style={{ color: '#cc0000', fontWeight: 'bold', alignSelf: 'center' }}>Are you sure?</span>
                                <button onClick={handleDelete}
                                    style={{ background: '#cc0000', color: 'white', border: 'none', padding: '10px 24px', borderRadius: '4px', cursor: 'pointer', fontSize: '15px' }}>
                                    Confirm
                                </button>
                                <button onClick={() => setShowConfirm(false)}
                                    style={{ background: '#555', color: 'white', border: 'none', padding: '10px 20px', borderRadius: '4px', cursor: 'pointer', fontSize: '15px' }}>
                                    Cancel
                                </button>
                            </>
                        )}
                    </div>
                )}
            </div>
        </div>
    );
}
