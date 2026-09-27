import { useCallback } from 'react';
import { api } from '../services/api';
import { postToOpener } from '../services/messaging';
import { useSearch } from '../hooks/useSearch';
import type { Course } from '../types/api';
import LoadStatus from '../components/LoadStatus';
import { styles } from '../styles/theme';

/** Popup window opened by the class frames; sends the chosen course back to its opener and closes. */
export default function CoursePickerPage() {
    const { results: courses, loading, error, run } = useSearch(useCallback(() => api.getCourses(), []));

    function selectCourse(course: Course) {
        postToOpener({ type: 'courseSelected', courseTitle: course.title, courseId: course.id });
        window.close();
    }

    return (
        <div style={styles.frame}>
            <h3 style={styles.frameHeading}>Select Course</h3>
            <button onClick={run} style={{ ...styles.smallButton, padding: '8px 20px', marginBottom: '16px' }}>Search</button>

            <LoadStatus loading={loading} error={error} />
            {!loading && courses?.length === 0 && <p style={styles.emptyText}>No courses found.</p>}

            {courses && courses.length > 0 && (
                <table style={styles.table}>
                    <thead>
                        <tr>
                            <th style={styles.compactTh}>Course Title</th>
                            <th style={styles.compactTh}>ID</th>
                        </tr>
                    </thead>
                    <tbody>
                        {courses.map(c => (
                            <tr key={c.id}>
                                <td style={styles.compactTd}>
                                    <a href="#" onClick={e => { e.preventDefault(); selectCourse(c); }} style={styles.textLink}>{c.title}</a>
                                </td>
                                <td style={styles.compactTd}>{c.id}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}
