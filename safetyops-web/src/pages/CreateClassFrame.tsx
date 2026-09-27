import { useState } from 'react';
import { api } from '../services/api';
import { postToParent, useAppMessages } from '../services/messaging';
import CalendarPicker, { CalendarBackdrop } from '../components/CalendarPicker';
import { colors, styles } from '../styles/theme';
import { validateClassForm } from './classValidation';

/** Runs inside the Training shell's iframe. The course comes from a picker popup window. */
export default function CreateClassFrame() {
    const [date, setDate] = useState('');
    const [location, setLocation] = useState('');
    const [courseTitle, setCourseTitle] = useState('');
    const [courseId, setCourseId] = useState('');
    const [showCalendar, setShowCalendar] = useState(false);
    const [duplicateIds, setDuplicateIds] = useState<number[]>([]);
    const [errors, setErrors] = useState<string[]>([]);

    useAppMessages(message => {
        if (message.type === 'courseSelected') {
            setCourseTitle(message.courseTitle);
            setCourseId(message.courseId);
        }
    });

    function readyToSave() {
        postToParent({ type: 'trainingReadyToSave', data: { courseTitle, courseId, classDate: date, location } });
    }

    async function handleCreate() {
        const errs = validateClassForm(courseId, date, location);
        setErrors(errs);
        if (errs.length > 0) return;

        try {
            const existing = await api.getTrainingClasses(courseTitle);
            const dups = existing.filter(c => c.classDate === date);
            if (dups.length > 0) {
                setDuplicateIds(dups.map(c => c.id));
                return;
            }
        } catch { /* proceed without duplicate check */ }

        readyToSave();
    }

    function openCoursePicker() {
        window.open('/training/course-picker', 'coursePicker', 'width=640,height=480,resizable=yes');
    }

    function handleContinue() {
        setDuplicateIds([]);
        readyToSave();
    }

    function handleGoToExisting() {
        const [first] = duplicateIds;
        setDuplicateIds([]);
        postToParent({ type: 'trainingGoToExisting', id: first });
    }

    function handleStartOver() {
        setDuplicateIds([]);
        setDate('');
        setLocation('');
        setCourseTitle('');
        setCourseId('');
        setErrors([]);
        postToParent({ type: 'trainingFormReset' });
    }

    return (
        <div style={styles.frame}>
            <h3 style={styles.frameHeading}>Create Training Class</h3>

            {errors.length > 0 && (
                <div role="alert" style={{ ...styles.errorText, marginBottom: '12px' }}>
                    {errors.map((e, i) => <div key={i}>{e}</div>)}
                </div>
            )}

            <div style={{ ...styles.frameField, position: 'relative' }}>
                <label style={styles.frameLabel}>Class Date</label>
                <input id="class-date" value={date} onChange={e => setDate(e.target.value)} onClick={() => setShowCalendar(true)}
                    placeholder="MM/DD/YYYY" style={{ ...styles.frameInput, width: '200px' }} />
                {showCalendar && <CalendarPicker onSelect={setDate} onClose={() => setShowCalendar(false)} />}
            </div>

            <div style={styles.frameField}>
                <label style={styles.frameLabel}>Location</label>
                <input id="class-location" value={location} onChange={e => setLocation(e.target.value)}
                    placeholder="Enter location" style={styles.frameInput} />
            </div>

            <div style={styles.field}>
                <label style={styles.frameLabel}>Course Title</label>
                <input id="course-title" value={courseTitle} onChange={e => setCourseTitle(e.target.value)}
                    placeholder="Select via picker..." style={{ ...styles.frameInput, backgroundColor: colors.readOnlyBg }} />
                <button id="course-picker-button" onClick={openCoursePicker} style={{ ...styles.smallButton, marginLeft: '8px' }}>
                    ...
                </button>
            </div>

            <button onClick={handleCreate} style={styles.primaryButton}>Create</button>

            {duplicateIds.length > 0 && (
                <div style={{ ...styles.alertWarning, marginTop: '20px' }}>
                    <p style={{ fontWeight: 'bold', color: '#856404', marginTop: 0 }}>
                        A class with this course and date already exists. How would you like to proceed?
                    </p>
                    <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
                        <button onClick={handleContinue} style={styles.smallButton}>Continue with create</button>
                        <button onClick={handleGoToExisting} style={{ ...styles.smallButton, background: colors.muted }}>Go to Existing</button>
                        <button onClick={handleStartOver} style={{ ...styles.smallButton, background: colors.danger }}>Start Over</button>
                    </div>
                </div>
            )}

            {showCalendar && <CalendarBackdrop onClose={() => setShowCalendar(false)} />}
        </div>
    );
}
