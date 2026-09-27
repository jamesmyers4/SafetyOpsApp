import { useState } from 'react';
import type { AppointmentForm } from '../hooks/useAppointmentForm';
import CalendarPicker, { CalendarBackdrop } from './CalendarPicker';
import { colors, styles } from '../styles/theme';

const readOnlyInput = { ...styles.frameInput, backgroundColor: '#f9f9f9' };

/** Date, person, and stressor fields shared by the appointment create and edit frames. */
export default function AppointmentFields({ form, workTaskPickerHeight }: { form: AppointmentForm; workTaskPickerHeight: string }) {
    const [showCalendar, setShowCalendar] = useState(false);

    function openPersonPicker() {
        window.open('/medical-surveillance/person-picker', 'personPicker', 'width=600,height=400,resizable=yes');
    }

    return (
        <>
            <div style={{ ...styles.frameField, position: 'relative', display: 'inline-block' }}>
                <label style={styles.frameLabel}>Appointment Date</label>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <input id="appointment-date" name="date" value={form.date} readOnly style={{ ...readOnlyInput, width: '180px' }} />
                    <svg viewBox="0 0 24 24" width="24" height="24" style={{ cursor: 'pointer', flexShrink: 0 }}
                        onClick={() => setShowCalendar(o => !o)}>
                        <path fill={colors.navy} d="M19 3h-1V1h-2v2H8V1H6v2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zM7 10h5v5H7z" />
                    </svg>
                </div>
                {showCalendar && <CalendarPicker top="32px" onSelect={form.setDate} onClose={() => setShowCalendar(false)} />}
            </div>

            <div style={styles.frameField}>
                <label style={styles.frameLabel}>Person Evaluated</label>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <input id="person-evaluated" value={form.personName} readOnly style={{ ...readOnlyInput, width: '280px' }} />
                    <button id="person-picker-button" onClick={openPersonPicker} style={{ ...styles.smallButton, padding: '8px 14px' }}>
                        ...
                    </button>
                </div>
            </div>

            <div style={styles.field}>
                <a href="#" role="link" onClick={e => { e.preventDefault(); form.openWorkTaskPicker(); }}
                    style={{ ...styles.boldLink, fontSize: '14px' }}>
                    Add Work Task(s)
                </a>
            </div>

            {form.showWorkTaskPicker && (
                <iframe
                    src="/medical-surveillance/work-task-picker"
                    style={{ width: '100%', height: workTaskPickerHeight, border: `1px solid ${colors.border}`, borderRadius: '4px', marginBottom: '16px' }}
                    title="Work Task Picker"
                />
            )}

            {form.stressors.length > 0 && (
                <div style={styles.field}>
                    <table style={styles.table}>
                        <thead>
                            <tr>
                                <th style={styles.compactTh}>Stressor ID</th>
                                <th style={styles.compactTh}>Stressor Name</th>
                                <th style={styles.compactTh}>Exam Type</th>
                            </tr>
                        </thead>
                        <tbody>
                            {form.stressors.map((s, i) => (
                                <tr key={s.stressorId}>
                                    <td style={styles.compactTd}>{s.stressorId}</td>
                                    <td style={styles.compactTd}>{s.stressorName}</td>
                                    <td style={styles.compactTd}>
                                        <select id={`exam-type-${i + 1}`} value={s.examType}
                                            onChange={e => form.setExamType(s.stressorId, e.target.value)}
                                            style={{ padding: '6px', border: `1px solid ${colors.border}`, borderRadius: '4px' }}>
                                            <option value="">Select Exam Type</option>
                                            {s.examTypeOptions.map(opt => <option key={opt} value={opt}>{opt}</option>)}
                                        </select>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {showCalendar && <CalendarBackdrop onClose={() => setShowCalendar(false)} />}
        </>
    );
}
