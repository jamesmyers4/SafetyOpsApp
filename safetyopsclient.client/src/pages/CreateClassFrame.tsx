import { useState, useEffect, useCallback } from 'react';
import { api } from '../services/api';

type DialogState = 'none' | 'duplicate';

function CalendarPicker({ onSelect, onClose }: { onSelect: (date: string) => void; onClose: () => void }) {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const days = Array.from({ length: daysInMonth }, (_, i) => i + 1);

    return (
        <div style={{ position: 'absolute', top: '40px', left: 0, background: 'white', border: '1px solid #ccc', borderRadius: '4px', padding: '12px', zIndex: 100, boxShadow: '0 4px 8px rgba(0,0,0,0.15)', minWidth: '240px' }}>
            <div style={{ fontWeight: 'bold', color: '#1a2744', marginBottom: '8px', textAlign: 'center' }}>
                {now.toLocaleString('default', { month: 'long' })} {year}
            </div>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px' }}>
                {days.map(d => (
                    <a key={d} href="#"
                        onClick={e => {
                            e.preventDefault();
                            const m = String(month + 1).padStart(2, '0');
                            const day = String(d).padStart(2, '0');
                            onSelect(`${m}/${day}/${year}`);
                            onClose();
                        }}
                        style={{ display: 'inline-block', width: '30px', textAlign: 'center', padding: '4px', cursor: 'pointer', color: '#1a2744', textDecoration: 'none', borderRadius: '3px' }}>
                        {d}
                    </a>
                ))}
            </div>
        </div>
    );
}

export default function CreateClassFrame() {
    const [date, setDate] = useState('');
    const [location, setLocation] = useState('');
    const [courseTitle, setCourseTitle] = useState('');
    const [courseId, setCourseId] = useState('');
    const [showCalendar, setShowCalendar] = useState(false);
    const [dialogState, setDialogState] = useState<DialogState>('none');
    const [duplicateIds, setDuplicateIds] = useState<number[]>([]);
    const [errors, setErrors] = useState<string[]>([]);

    const handleMessage = useCallback((event: MessageEvent) => {
        if (event.data?.type === 'courseSelected') {
            setCourseTitle(event.data.courseTitle ?? '');
            setCourseId(event.data.courseId ?? '');
        }
    }, []);

    useEffect(() => {
        window.addEventListener('message', handleMessage);
        return () => window.removeEventListener('message', handleMessage);
    }, [handleMessage]);

    function validateDate(d: string): string | null {
        if (!d) return 'Class Date is required.';
        const parsed = new Date(d);
        if (isNaN(parsed.getTime())) return 'Invalid date. Please enter a valid date.';
        if (parsed > new Date()) return 'Future dates are not allowed.';
        return null;
    }

    async function handleCreate() {
        const errs: string[] = [];
        if (!courseId) errs.push('Course ID is required.');
        const dateErr = validateDate(date);
        if (dateErr) errs.push(dateErr);
        if (!location.trim()) errs.push('Specific location is required');
        setErrors(errs);
        if (errs.length > 0) return;

        try {
            const existing = await api.getTrainingClasses(courseTitle);
            const dups = existing.filter(c => c.classDate === date);
            if (dups.length > 0) {
                setDuplicateIds(dups.map(c => c.id));
                setDialogState('duplicate');
                return;
            }
        } catch { /* proceed without duplicate check */ }

        window.parent.postMessage({
            type: 'trainingReadyToSave',
            data: { courseTitle, courseId, classDate: date, location },
        }, '*');
    }

    function openCoursePicker() {
        window.open('/n/safetyops/training/course-picker', 'coursePicker', 'width=640,height=480,resizable=yes');
    }

    function handleContinue() {
        setDialogState('none');
        window.parent.postMessage({
            type: 'trainingReadyToSave',
            data: { courseTitle, courseId, classDate: date, location },
        }, '*');
    }

    function handleGoToExisting() {
        setDialogState('none');
        window.parent.postMessage({ type: 'trainingGoToExisting', id: duplicateIds[0] }, '*');
    }

    function handleStartOver() {
        setDialogState('none');
        setDate('');
        setLocation('');
        setCourseTitle('');
        setCourseId('');
        setErrors([]);
        window.parent.postMessage({ type: 'trainingFormReset' }, '*');
    }

    return (
        <div style={{ fontFamily: 'Arial, sans-serif', padding: '20px', background: 'white', minHeight: '100vh' }}>
            <h3 style={{ color: '#1a2744', marginTop: 0 }}>Create Training Class</h3>

            {errors.length > 0 && (
                <div style={{ color: 'red', marginBottom: '12px', fontSize: '14px' }}>
                    {errors.map((e, i) => <div key={i}>{e}</div>)}
                </div>
            )}

            <div style={{ marginBottom: '16px', position: 'relative' }}>
                <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Class Date</label>
                <input
                    id="txtClassDate"
                    value={date}
                    onChange={e => setDate(e.target.value)}
                    onClick={() => setShowCalendar(true)}
                    placeholder="MM/DD/YYYY"
                    style={{ padding: '8px', width: '200px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }}
                />
                {showCalendar && (
                    <CalendarPicker
                        onSelect={d => { setDate(d); setShowCalendar(false); }}
                        onClose={() => setShowCalendar(false)}
                    />
                )}
            </div>

            <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Location</label>
                <input
                    id="txtLocation"
                    value={location}
                    onChange={e => setLocation(e.target.value)}
                    placeholder="Enter location"
                    style={{ padding: '8px', width: '350px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }}
                />
            </div>

            <div style={{ marginBottom: '20px' }}>
                <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Course Title</label>
                <input
                    id="txtCourseTitle"
                    value={courseTitle}
                    onChange={e => setCourseTitle(e.target.value)}
                    placeholder="Select via picker..."
                    style={{ padding: '8px', width: '350px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px', backgroundColor: '#f5f5f5' }}
                />
                <button
                    id="txtCourseTitle_HGWselList"
                    onClick={openCoursePicker}
                    style={{ marginLeft: '8px', background: '#1a2744', color: 'white', border: 'none', padding: '8px 16px', borderRadius: '4px', cursor: 'pointer' }}
                >
                    ...
                </button>
            </div>

            <button
                onClick={handleCreate}
                style={{ background: '#1a2744', color: 'white', border: 'none', padding: '10px 28px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer' }}
            >
                Create
            </button>

            {dialogState === 'duplicate' && (
                <div style={{ marginTop: '20px', padding: '16px', background: '#fff3cd', border: '1px solid #ffc107', borderRadius: '4px' }}>
                    <p style={{ fontWeight: 'bold', color: '#856404', marginTop: 0 }}>
                        A class with this course and date already exists. How would you like to proceed?
                    </p>
                    <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
                        <button onClick={handleContinue}
                            style={{ background: '#1a2744', color: 'white', border: 'none', padding: '8px 16px', borderRadius: '4px', cursor: 'pointer' }}>
                            Continue with create
                        </button>
                        <button onClick={handleGoToExisting}
                            style={{ background: '#555', color: 'white', border: 'none', padding: '8px 16px', borderRadius: '4px', cursor: 'pointer' }}>
                            Go to Existing
                        </button>
                        <button onClick={handleStartOver}
                            style={{ background: '#cc0000', color: 'white', border: 'none', padding: '8px 16px', borderRadius: '4px', cursor: 'pointer' }}>
                            Start Over
                        </button>
                    </div>
                </div>
            )}

            {showCalendar && (
                <div style={{ position: 'fixed', inset: 0, zIndex: 50 }} onClick={() => setShowCalendar(false)} />
            )}
        </div>
    );
}
