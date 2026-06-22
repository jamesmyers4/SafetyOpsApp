import { useState, useEffect, useCallback } from 'react';
import { api } from '../services/api';

interface Stressor {
    stressorId: string;
    stressorName: string;
    examType: string;
    examTypeOptions: string[];
}

const ALL_EXAM_TYPES = ['Initial', 'Periodic', 'Exit', 'Return to Duty', 'Special'];

function CalendarPicker({ onSelect, onClose }: { onSelect: (date: string) => void; onClose: () => void }) {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    return (
        <div style={{ position: 'absolute', background: 'white', border: '1px solid #ccc', borderRadius: '4px', padding: '12px', zIndex: 100, boxShadow: '0 4px 8px rgba(0,0,0,0.15)', minWidth: '240px', top: '32px', left: 0 }}>
            <div style={{ fontWeight: 'bold', color: '#1a2744', marginBottom: '8px', textAlign: 'center' }}>
                {now.toLocaleString('default', { month: 'long' })} {year}
            </div>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px' }}>
                {Array.from({ length: daysInMonth }, (_, i) => i + 1).map(d => (
                    <a key={d} href="#"
                        onClick={e => {
                            e.preventDefault();
                            const mm = String(month + 1).padStart(2, '0');
                            const dd = String(d).padStart(2, '0');
                            onSelect(`${mm}/${dd}/${year}`);
                            onClose();
                        }}
                        style={{ display: 'inline-block', width: '30px', textAlign: 'center', padding: '4px', cursor: 'pointer', color: '#1a2744', textDecoration: 'none' }}>
                        {d}
                    </a>
                ))}
            </div>
        </div>
    );
}

export default function EditOMSSFrame() {
    const [appointmentId, setAppointmentId] = useState<number | null>(null);
    const [date, setDate] = useState('');
    const [showCalendar, setShowCalendar] = useState(false);
    const [personName, setPersonName] = useState('');
    const [personId, setPersonId] = useState(0);
    const [stressors, setStressors] = useState<Stressor[]>([]);
    const [showWorkTaskPicker, setShowWorkTaskPicker] = useState(false);
    const [successMsg, setSuccessMsg] = useState('');
    const [errors, setErrors] = useState<string[]>([]);

    useEffect(() => {
        const params = new URLSearchParams(window.location.search);
        const id = Number(params.get('id'));
        if (id) {
            setAppointmentId(id);
            api.getOmssAppointment(id).then(appt => {
                setDate(appt.date);
                setPersonName(appt.personName);
                setPersonId(appt.personId);
                setStressors(appt.stressors.map(s => ({
                    stressorId: s.stressorId,
                    stressorName: s.stressorName,
                    examType: s.examType,
                    examTypeOptions: ALL_EXAM_TYPES,
                })));
            }).catch(console.error);
        }
    }, []);

    const handleMessage = useCallback((event: MessageEvent) => {
        if (event.data?.type === 'personSelected') {
            setPersonName(event.data.name ?? '');
            setPersonId(event.data.id ?? 0);
        } else if (event.data?.type === 'workTasksSelected') {
            const tasks: { stressorId: string; stressorName: string; examTypeOptions: string[] }[] = event.data.tasks ?? [];
            setStressors(prev => {
                const newOnes = tasks
                    .filter(t => !prev.some(p => p.stressorId === t.stressorId))
                    .map(t => ({ stressorId: t.stressorId, stressorName: t.stressorName, examType: '', examTypeOptions: t.examTypeOptions }));
                return [...prev, ...newOnes];
            });
            setShowWorkTaskPicker(false);
        }
    }, []);

    useEffect(() => {
        window.addEventListener('message', handleMessage);
        return () => window.removeEventListener('message', handleMessage);
    }, [handleMessage]);

    function openPersonPicker() {
        window.open('/n/safetyops/omss/person-picker', 'personPicker', 'width=600,height=400,resizable=yes');
    }

    function setExamType(stressorId: string, value: string) {
        setStressors(prev => prev.map(s => s.stressorId === stressorId ? { ...s, examType: value } : s));
    }

    async function handleUpdate() {
        if (!appointmentId) return;
        try {
            await api.updateOmssAppointment(appointmentId, {
                date, personName, personId,
                stressors: stressors.map(s => ({ stressorId: s.stressorId, stressorName: s.stressorName, examType: s.examType })),
            });
            setSuccessMsg('Record updated successfully');
            window.parent.postMessage({ type: 'omssUpdated', message: 'Record updated successfully' }, '*');
        } catch (e) {
            console.error(e);
        }
    }

    function handleCancel() {
        window.parent.postMessage({ type: 'omssEditCancelled' }, '*');
    }

    return (
        <div style={{ fontFamily: 'Arial, sans-serif', padding: '20px', background: 'white', minHeight: '100%' }}>
            <h3 style={{ color: '#1a2744', marginTop: 0 }}>Edit Medical Surveillance Record</h3>

            {successMsg && (
                <div style={{ background: '#d4edda', color: '#155724', padding: '12px', borderRadius: '4px', marginBottom: '12px', fontSize: '14px' }}>
                    {successMsg}
                </div>
            )}

            <div style={{ marginBottom: '16px', position: 'relative', display: 'inline-block' }}>
                <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Appointment Date</label>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <input id="txtDate" name="Date" value={date} readOnly
                        style={{ padding: '8px', width: '180px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px', backgroundColor: '#f9f9f9' }} />
                    <svg viewBox="0 0 24 24" width="24" height="24" style={{ cursor: 'pointer', flexShrink: 0 }}
                        onClick={() => setShowCalendar(o => !o)}>
                        <path fill="#1a2744" d="M19 3h-1V1h-2v2H8V1H6v2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zM7 10h5v5H7z" />
                    </svg>
                </div>
                {showCalendar && (
                    <CalendarPicker
                        onSelect={d => { setDate(d); setShowCalendar(false); }}
                        onClose={() => setShowCalendar(false)}
                    />
                )}
            </div>

            <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Person Evaluated</label>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <input id="txtPerson" value={personName} readOnly
                        style={{ padding: '8px', width: '280px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px', backgroundColor: '#f9f9f9' }} />
                    <button id="txtPerson_HGWselList" onClick={openPersonPicker}
                        style={{ background: '#1a2744', color: 'white', border: 'none', padding: '8px 14px', borderRadius: '4px', cursor: 'pointer' }}>
                        ...
                    </button>
                </div>
            </div>

            <div style={{ marginBottom: '20px' }}>
                <a href="#" role="link"
                    onClick={e => { e.preventDefault(); setShowWorkTaskPicker(true); }}
                    style={{ color: '#1a2744', fontWeight: 'bold', textDecoration: 'underline', cursor: 'pointer', fontSize: '14px' }}>
                    Add Work Task(s)
                </a>
            </div>

            {showWorkTaskPicker && (
                <iframe
                    src="/n/safetyops/omss/work-task-picker"
                    style={{ width: '100%', height: '300px', border: '1px solid #ccc', borderRadius: '4px', marginBottom: '16px' }}
                    title="Work Task Picker"
                />
            )}

            {stressors.length > 0 && (
                <div style={{ marginBottom: '20px' }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                        <thead>
                            <tr>
                                <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>Stressor ID</th>
                                <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>Stressor Name</th>
                                <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>Exam Type</th>
                            </tr>
                        </thead>
                        <tbody>
                            {stressors.map((s, i) => (
                                <tr key={s.stressorId}>
                                    <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>{s.stressorId}</td>
                                    <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>{s.stressorName}</td>
                                    <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>
                                        <select id={`ddl${i + 1}`} value={s.examType}
                                            onChange={e => setExamType(s.stressorId, e.target.value)}
                                            style={{ padding: '6px', border: '1px solid #ccc', borderRadius: '4px' }}>
                                            <option value="">Select Exam Type</option>
                                            {s.examTypeOptions.map(opt => (
                                                <option key={opt} value={opt}>{opt}</option>
                                            ))}
                                        </select>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            <div style={{ display: 'flex', gap: '10px', marginTop: '8px' }}>
                <button onClick={handleUpdate}
                    style={{ background: '#1a2744', color: 'white', border: 'none', padding: '10px 28px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer' }}>
                    Update
                </button>
                <button onClick={handleCancel}
                    style={{ background: '#555', color: 'white', border: 'none', padding: '10px 20px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer' }}>
                    Cancel
                </button>
            </div>

            {showCalendar && (
                <div style={{ position: 'fixed', inset: 0, zIndex: 50 }} onClick={() => setShowCalendar(false)} />
            )}
        </div>
    );
}
