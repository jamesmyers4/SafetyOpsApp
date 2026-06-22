import { useState, useEffect, useCallback } from 'react';
import { api } from '../services/api';

interface Stressor {
    stressorId: string;
    stressorName: string;
    examType: string;
    examTypeOptions: string[];
}

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

export default function CreateOMSSFrame() {
    const [date, setDate] = useState('');
    const [showCalendar, setShowCalendar] = useState(false);
    const [personName, setPersonName] = useState('');
    const [personId, setPersonId] = useState(0);
    const [stressors, setStressors] = useState<Stressor[]>([]);
    const [showWorkTaskPicker, setShowWorkTaskPicker] = useState(false);
    const [errors, setErrors] = useState<string[]>([]);

    const handleMessage = useCallback((event: MessageEvent) => {
        if (event.data?.type === 'personSelected') {
            setPersonName(event.data.name ?? '');
            setPersonId(event.data.id ?? 0);
        } else if (event.data?.type === 'workTasksSelected') {
            const tasks: { stressorId: string; stressorName: string; examTypeOptions: string[] }[] = event.data.tasks ?? [];
            setStressors(prev => {
                const newOnes = tasks.map(t => ({
                    stressorId: t.stressorId,
                    stressorName: t.stressorName,
                    examType: '',
                    examTypeOptions: t.examTypeOptions,
                })).filter(t => !prev.some(p => p.stressorId === t.stressorId));
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

    async function handleCreate() {
        try {
            const result = await api.createOmssAppointment({
                date,
                personName,
                personId,
                stressors: stressors.map(s => ({ stressorId: s.stressorId, stressorName: s.stressorName, examType: s.examType })),
            });
            window.parent.location.href = `/n/safetyops/omss/appointment/${result.id}`;
        } catch (e) {
            console.error(e);
        }
    }

    return (
        <div style={{ fontFamily: 'Arial, sans-serif', padding: '20px', background: 'white', minHeight: '100vh' }}>
            <h3 style={{ color: '#1a2744', marginTop: 0 }}>Create Medical Surveillance Record</h3>

            {errors.length > 0 && (
                <div style={{ color: 'red', marginBottom: '12px', fontSize: '14px' }}>
                    {errors.map((e, i) => <div key={i}>{e}</div>)}
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
                    style={{ width: '100%', height: '350px', border: '1px solid #ccc', borderRadius: '4px', marginBottom: '16px' }}
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
                                        <select
                                            id={`ddl${i + 1}`}
                                            value={s.examType}
                                            onChange={e => setExamType(s.stressorId, e.target.value)}
                                            style={{ padding: '6px', border: '1px solid #ccc', borderRadius: '4px' }}
                                        >
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

            <button onClick={handleCreate}
                style={{ background: '#1a2744', color: 'white', border: 'none', padding: '10px 28px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer' }}>
                Create
            </button>

            {showCalendar && (
                <div style={{ position: 'fixed', inset: 0, zIndex: 50 }} onClick={() => setShowCalendar(false)} />
            )}
        </div>
    );
}
