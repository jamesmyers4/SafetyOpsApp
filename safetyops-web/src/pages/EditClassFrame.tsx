import { useCallback, useState } from 'react';
import { api } from '../services/api';
import { postToParent, useAppMessages } from '../services/messaging';
import { useSearch } from '../hooks/useSearch';
import type { TrainingClass } from '../types/api';
import CalendarPicker, { CalendarBackdrop } from '../components/CalendarPicker';
import LoadStatus from '../components/LoadStatus';
import { colors, styles } from '../styles/theme';
import { validateClassForm } from './classValidation';

/** Runs inside the Training shell's iframe: search for a class, then edit it. */
export default function EditClassFrame() {
    const [searchTerm, setSearchTerm] = useState('');
    const search = useSearch(useCallback(() => api.getTrainingClasses(searchTerm), [searchTerm]));

    const [editClass, setEditClass] = useState<TrainingClass | null>(null);
    const [courseTitle, setCourseTitle] = useState('');
    const [courseId, setCourseId] = useState('');
    const [classDate, setClassDate] = useState('');
    const [location, setLocation] = useState('');
    const [showCalendar, setShowCalendar] = useState(false);
    const [errors, setErrors] = useState<string[]>([]);
    const [showDuplicate, setShowDuplicate] = useState(false);

    useAppMessages(message => {
        if (message.type === 'courseSelected') {
            setCourseTitle(message.courseTitle);
            setCourseId(message.courseId);
        }
    });

    function openForEdit(cls: TrainingClass) {
        setEditClass(cls);
        setCourseTitle(cls.courseTitle);
        setCourseId(cls.courseId);
        setClassDate(cls.classDate);
        setLocation(cls.location);
        setErrors([]);
        setShowDuplicate(false);
    }

    function closeEditor() {
        setEditClass(null);
        setErrors([]);
        setShowDuplicate(false);
        postToParent({ type: 'trainingFormReset' });
    }

    function postReadyToSave() {
        postToParent({
            type: 'trainingReadyToSave',
            data: { id: editClass?.id, courseTitle, courseId, classDate, location },
            isUpdate: true,
        });
    }

    async function handleUpdate() {
        const errs = validateClassForm(courseId, classDate, location);
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
        window.open('/training/course-picker', 'coursePicker', 'width=640,height=480,resizable=yes');
    }

    if (!editClass) {
        const results = search.results;
        return (
            <div style={styles.frame}>
                <h3 style={styles.frameHeading}>
                    <a href="#" style={{ color: colors.navy, textDecoration: 'underline' }}>Find / Search Classes</a>
                </h3>
                <div style={{ ...styles.frameField, display: 'flex', gap: '8px' }}>
                    <input id="class-search" value={searchTerm} onChange={e => setSearchTerm(e.target.value)}
                        onKeyDown={e => { if (e.key === 'Enter') search.run(); }}
                        placeholder="Search by course or location..." style={{ ...styles.frameInput, width: '300px' }} />
                    <button onClick={search.run} style={{ ...styles.smallButton, padding: '8px 20px' }}>Search</button>
                </div>

                <LoadStatus loading={search.loading} error={search.error} />
                {!search.loading && results?.length === 0 && <p style={styles.emptyText}>No results found.</p>}

                {results && results.length > 0 && (
                    <table style={styles.table}>
                        <thead>
                            <tr>
                                <th style={styles.compactTh}>Course</th>
                                <th style={styles.compactTh}>Date</th>
                                <th style={styles.compactTh}>Location</th>
                            </tr>
                        </thead>
                        <tbody>
                            {results.map(cls => (
                                <tr key={cls.id}>
                                    <td style={styles.compactTd}>
                                        <a href="#" onClick={e => { e.preventDefault(); openForEdit(cls); }} style={styles.textLink}>
                                            {cls.courseTitle}
                                        </a>
                                    </td>
                                    <td style={styles.compactTd}>{cls.classDate}</td>
                                    <td style={styles.compactTd}>{cls.location}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>
        );
    }

    return (
        <div style={styles.frame}>
            <h3 style={styles.frameHeading}>Edit Training Class</h3>

            {errors.length > 0 && (
                <div role="alert" style={{ ...styles.errorText, marginBottom: '12px' }}>
                    {errors.map((e, i) => <div key={i}>{e}</div>)}
                </div>
            )}

            <div style={{ ...styles.frameField, position: 'relative' }}>
                <label style={styles.frameLabel}>Class Date</label>
                <input id="class-date" value={classDate} onChange={e => setClassDate(e.target.value)} onClick={() => setShowCalendar(true)}
                    placeholder="MM/DD/YYYY" style={{ ...styles.frameInput, width: '200px' }} />
                {showCalendar && <CalendarPicker onSelect={setClassDate} onClose={() => setShowCalendar(false)} />}
            </div>

            <div style={styles.frameField}>
                <label style={styles.frameLabel}>Location</label>
                <input id="class-location" value={location} onChange={e => setLocation(e.target.value)} style={styles.frameInput} />
            </div>

            <div style={styles.field}>
                <label style={styles.frameLabel}>Course Title</label>
                <input id="course-title" value={courseTitle} onChange={e => setCourseTitle(e.target.value)}
                    style={{ ...styles.frameInput, backgroundColor: colors.readOnlyBg }} />
                <button id="course-picker-button" onClick={openCoursePicker} style={{ ...styles.smallButton, marginLeft: '8px' }}>
                    ...
                </button>
            </div>

            <div style={{ display: 'flex', gap: '10px', marginBottom: '16px' }}>
                <button onClick={handleUpdate} style={styles.primaryButton}>Update</button>
                <button onClick={closeEditor} style={styles.secondaryButton}>Cancel</button>
            </div>

            {showDuplicate && (
                <div style={styles.alertWarning}>
                    <p style={{ fontWeight: 'bold', color: '#856404', marginTop: 0 }}>
                        A duplicate record may exist. How would you like to proceed?
                    </p>
                    <div style={{ display: 'flex', gap: '10px' }}>
                        <button onClick={() => { setShowDuplicate(false); postReadyToSave(); }} style={styles.smallButton}>
                            Continue with update
                        </button>
                    </div>
                </div>
            )}

            {showCalendar && <CalendarBackdrop onClose={() => setShowCalendar(false)} />}
        </div>
    );
}
