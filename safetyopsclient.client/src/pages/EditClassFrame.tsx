import { useState, useEffect, useCallback } from 'react';
import { api } from '../services/api';

interface TrainingClass {
    id: number;
    courseTitle: string;
    courseId: string;
    classDate: string;
    location: string;
}

type ViewState = 'search' | 'edit';

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

export default function EditClassFrame() {
    const [viewState, setViewState] = useState<ViewState>('search');
    const [searchTerm, setSearchTerm] = useState('');
    const [searchResults, setSearchResults] = useState<TrainingClass[]>([]);
    const [searched, setSearched] = useState(false);

    const [editClass, setEditClass] = useState<TrainingClass | null>(null);
    const [courseTitle, setCourseTitle] = useState('');
    const [courseId, setCourseId] = useState('');
    const [classDate, setClassDate] = useState('');
    const [location, setLocation] = useState('');
    const [showCalendar, setShowCalendar] = useState(false);
    const [errors, setErrors] = useState<string[]>([]);
    const [showDuplicate, setShowDuplicate] = useState(false);

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

    async function handleSearch() {
        try {
            const results = await api.getTrainingClasses(searchTerm);
            setSearchResults(results);
            setSearched(true);
        } catch (e) {
            console.error(e);
        }
    }

    function openForEdit(cls: TrainingClass) {
        setEditClass(cls);
        setCourseTitle(cls.courseTitle);
        setCourseId(cls.courseId);
        setClassDate(cls.classDate);
        setLocation(cls.location);
        setErrors([]);
        setShowDuplicate(false);
        setViewState('edit');
    }

    function validateDate(d: string): string | null {
        if (!d) return 'Class Date is required.';
        const parsed = new Date(d);
        if (isNaN(parsed.getTime())) return 'Invalid date. Please enter a valid date.';
        if (parsed > new Date()) return 'Future dates are not allowed.';
        return null;
    }

    function postReadyToSave() {
        window.parent.postMessage({
            type: 'trainingReadyToSave',
            data: { id: editClass?.id, courseTitle, courseId, classDate, location },
            isUpdate: true,
        }, '*');
    }

    async function handleUpdate() {
        const errs: string[] = [];
        if (!courseTitle.trim()) errs.push('Course ID is required.');
        const dateErr = validateDate(classDate);
        if (dateErr) errs.push(dateErr);
        if (!location.trim()) errs.push('Specific location is required');
        setErrors(errs);
        if (errs.length > 0) return;

        try {
            const existing = await api.getTrainingClasses(courseTitle);
            const dups = existing.filter(c => c.classDate === classDate && c.id !== editClass?.id);
            if (dups.length > 0) { setShowDuplicate(true); return; }
        } catch { /* proceed */ }

        postReadyToSave();
    }

    function openCoursePicker() {
        window.open('/n/safetyops/training/course-picker', 'coursePicker', 'width=640,height=480,resizable=yes');
    }

    return (
        <div style={{ fontFamily: 'Arial, sans-serif', padding: '20px', background: 'white', minHeight: '100vh' }}>
            {viewState === 'search' && (
                <>
                    <h3 style={{ color: '#1a2744', marginTop: 0 }}>
                        <a href="#" style={{ color: '#1a2744', textDecoration: 'underline' }}>Find / Search Classes</a>
                    </h3>
                    <div style={{ marginBottom: '16px', display: 'flex', gap: '8px' }}>
                        <input
                            id="txtSearchClassName"
                            value={searchTerm}
                            onChange={e => setSearchTerm(e.target.value)}
                            onKeyDown={e => { if (e.key === 'Enter') handleSearch(); }}
                            placeholder="Search by course or location..."
                            style={{ padding: '8px', width: '300px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }}
                        />
                        <button onClick={handleSearch}
                            style={{ background: '#1a2744', color: 'white', border: 'none', padding: '8px 20px', borderRadius: '4px', cursor: 'pointer' }}>
                            Search
                        </button>
                    </div>

                    {searched && searchResults.length === 0 && (
                        <p style={{ color: '#666' }}>No results found.</p>
                    )}

                    {searchResults.length > 0 && (
                        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                            <thead>
                                <tr>
                                    <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>Course</th>
                                    <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>Date</th>
                                    <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>Location</th>
                                </tr>
                            </thead>
                            <tbody>
                                {searchResults.map(cls => (
                                    <tr key={cls.id}>
                                        <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>
                                            <a href="#" onClick={e => { e.preventDefault(); openForEdit(cls); }}
                                                style={{ color: '#1a2744', textDecoration: 'underline', cursor: 'pointer' }}>
                                                {cls.courseTitle}
                                            </a>
                                        </td>
                                        <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>{cls.classDate}</td>
                                        <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>{cls.location}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}
                </>
            )}

            {viewState === 'edit' && (
                <>
                    <h3 style={{ color: '#1a2744', marginTop: 0 }}>Edit Training Class</h3>

                    {errors.length > 0 && (
                        <div style={{ color: 'red', marginBottom: '12px', fontSize: '14px' }}>
                            {errors.map((e, i) => <div key={i}>{e}</div>)}
                        </div>
                    )}

                    <div style={{ marginBottom: '16px', position: 'relative' }}>
                        <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Class Date</label>
                        <input
                            id="txtClassDate"
                            value={classDate}
                            onChange={e => setClassDate(e.target.value)}
                            onClick={() => setShowCalendar(true)}
                            placeholder="MM/DD/YYYY"
                            style={{ padding: '8px', width: '200px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }}
                        />
                        {showCalendar && (
                            <CalendarPicker
                                onSelect={d => { setClassDate(d); setShowCalendar(false); }}
                                onClose={() => setShowCalendar(false)}
                            />
                        )}
                    </div>

                    <div style={{ marginBottom: '16px' }}>
                        <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Location</label>
                        <input id="txtLocation" value={location} onChange={e => setLocation(e.target.value)}
                            style={{ padding: '8px', width: '350px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }} />
                    </div>

                    <div style={{ marginBottom: '20px' }}>
                        <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Course Title</label>
                        <input id="txtCourseTitle" value={courseTitle} onChange={e => setCourseTitle(e.target.value)}
                            style={{ padding: '8px', width: '350px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px', backgroundColor: '#f5f5f5' }} />
                        <button id="txtCourseTitle_HGWselList" onClick={openCoursePicker}
                            style={{ marginLeft: '8px', background: '#1a2744', color: 'white', border: 'none', padding: '8px 16px', borderRadius: '4px', cursor: 'pointer' }}>
                            ...
                        </button>
                    </div>

                    <div style={{ display: 'flex', gap: '10px', marginBottom: '16px' }}>
                        <button onClick={handleUpdate}
                            style={{ background: '#1a2744', color: 'white', border: 'none', padding: '10px 28px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer' }}>
                            Update
                        </button>
                        <button onClick={() => { setViewState('search'); setErrors([]); setShowDuplicate(false); window.parent.postMessage({ type: 'trainingFormReset' }, '*'); }}
                            style={{ background: '#555', color: 'white', border: 'none', padding: '10px 20px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer' }}>
                            Cancel
                        </button>
                    </div>

                    {showDuplicate && (
                        <div style={{ padding: '16px', background: '#fff3cd', border: '1px solid #ffc107', borderRadius: '4px' }}>
                            <p style={{ fontWeight: 'bold', color: '#856404', marginTop: 0 }}>
                                A duplicate record may exist. How would you like to proceed?
                            </p>
                            <div style={{ display: 'flex', gap: '10px' }}>
                                <button onClick={() => { setShowDuplicate(false); postReadyToSave(); }}
                                    style={{ background: '#1a2744', color: 'white', border: 'none', padding: '8px 16px', borderRadius: '4px', cursor: 'pointer' }}>
                                    Continue with update
                                </button>
                            </div>
                        </div>
                    )}
                </>
            )}

            {showCalendar && (
                <div style={{ position: 'fixed', inset: 0, zIndex: 50 }} onClick={() => setShowCalendar(false)} />
            )}
        </div>
    );
}
