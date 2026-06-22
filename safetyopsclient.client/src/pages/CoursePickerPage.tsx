import { useState, useEffect } from 'react';
import { api } from '../services/api';

interface Course {
    id: string;
    title: string;
}

export default function CoursePickerPage() {
    const [courses, setCourses] = useState<Course[]>([]);
    const [searched, setSearched] = useState(false);

    async function handleSearch() {
        try {
            const results = await api.getCourses();
            setCourses(results);
            setSearched(true);
        } catch (e) {
            console.error(e);
        }
    }

    function selectCourse(course: Course) {
        if (window.opener) {
            window.opener.postMessage({ type: 'courseSelected', courseTitle: course.title, courseId: course.id }, '*');
        }
        window.close();
    }

    return (
        <div style={{ fontFamily: 'Arial, sans-serif', padding: '20px', background: 'white', minHeight: '100vh' }}>
            <h3 style={{ color: '#1a2744', marginTop: 0 }}>Select Course</h3>
            <button
                onClick={handleSearch}
                style={{ background: '#1a2744', color: 'white', border: 'none', padding: '8px 20px', borderRadius: '4px', cursor: 'pointer', marginBottom: '16px' }}
            >
                Search
            </button>

            {searched && courses.length === 0 && <p style={{ color: '#666' }}>No courses found.</p>}

            {courses.length > 0 && (
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead>
                        <tr>
                            <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>Course Title</th>
                            <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>ID</th>
                        </tr>
                    </thead>
                    <tbody>
                        {courses.map(c => (
                            <tr key={c.id}>
                                <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>
                                    <a href="#" onClick={e => { e.preventDefault(); selectCourse(c); }}
                                        style={{ color: '#1a2744', textDecoration: 'underline', cursor: 'pointer' }}>
                                        {c.title}
                                    </a>
                                </td>
                                <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>{c.id}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}
