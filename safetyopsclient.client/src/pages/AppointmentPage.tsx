import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import NavBar from '../components/NavBar';

interface Appointment {
    id: number;
    date: string;
    personName: string;
    stressors: { stressorId: string; stressorName: string; examType: string }[];
}

export default function AppointmentPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [appt, setAppt] = useState<Appointment | null>(null);
    const [showConfirm, setShowConfirm] = useState(false);
    const [deleted, setDeleted] = useState(false);

    useEffect(() => {
        if (id) {
            api.getOmssAppointment(Number(id)).then(setAppt).catch(console.error);
        }
    }, [id]);

    async function handleDelete() {
        try {
            await api.deleteOmssAppointment(Number(id));
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
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/omss'); }} style={navLink}>
                    Medical Surveillance (OMSS)
                </a>
            } />
            <div style={{ padding: '40px 60px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '20px' }}>Appointment Details</h2>

                {deleted ? (
                    <div style={{ background: '#f8d7da', color: '#721c24', padding: '12px 20px', borderRadius: '4px', marginBottom: '24px', fontSize: '14px' }}>
                        Appointment deleted successfully.
                    </div>
                ) : (
                    appt && (
                        <div style={{ background: 'white', padding: '24px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)', marginBottom: '24px' }}>
                            <p><strong>ID:</strong> {appt.id}</p>
                            <p><strong>Date:</strong> {appt.date}</p>
                            <p><strong>Person Evaluated:</strong> {appt.personName}</p>
                            {appt.stressors.length > 0 && (
                                <>
                                    <p><strong>Stressors:</strong></p>
                                    <ul>
                                        {appt.stressors.map(s => (
                                            <li key={s.stressorId}>{s.stressorName} — {s.examType}</li>
                                        ))}
                                    </ul>
                                </>
                            )}
                        </div>
                    )
                )}

                {!deleted && (
                    <div style={{ display: 'flex', gap: '12px' }}>
                        {!showConfirm && (
                            <button onClick={() => setShowConfirm(true)}
                                style={{ background: '#cc0000', color: 'white', border: 'none', padding: '10px 24px', borderRadius: '4px', cursor: 'pointer', fontSize: '15px' }}>
                                Delete Appointment
                            </button>
                        )}
                        {showConfirm && (
                            <>
                                <span style={{ color: '#cc0000', fontWeight: 'bold', alignSelf: 'center' }}>Confirm deletion?</span>
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
