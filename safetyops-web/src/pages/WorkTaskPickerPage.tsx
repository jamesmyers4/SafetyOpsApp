import { useCallback, useState } from 'react';
import { api } from '../services/api';
import { postToParent } from '../services/messaging';
import { useSearch } from '../hooks/useSearch';
import LoadStatus from '../components/LoadStatus';
import { styles } from '../styles/theme';

/** Nested iframe inside the appointment frames; posts the stressors of the checked work tasks to its parent. */
export default function WorkTaskPickerPage() {
    const { results: tasks, loading, error, run } = useSearch(useCallback(() => api.getWorkTasks(), []));
    const [selected, setSelected] = useState<Set<string>>(new Set());

    function toggleTask(id: string) {
        setSelected(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id); else next.add(id);
            return next;
        });
    }

    function handleSave() {
        const stressors = (tasks ?? [])
            .filter(t => selected.has(t.id))
            .flatMap(t => t.stressors.map(s => ({ ...s, examTypeOptions: t.examTypeOptions })));
        postToParent({ type: 'workTasksSelected', tasks: stressors });
    }

    return (
        <div style={{ ...styles.frame, padding: '16px', minHeight: '100%' }}>
            <h4 style={styles.frameHeading}>Select Work Tasks</h4>
            <button onClick={run} style={{ ...styles.smallButton, padding: '8px 20px', marginBottom: '12px' }}>Search</button>

            <LoadStatus loading={loading} error={error} />
            {!loading && tasks?.length === 0 && <p style={styles.emptyText}>No work tasks found.</p>}

            {tasks && tasks.length > 0 && (
                <>
                    <table style={{ ...styles.table, marginBottom: '12px' }}>
                        <thead>
                            <tr>
                                <th style={{ ...styles.compactTh, padding: '8px', width: '40px' }}>Select</th>
                                <th style={{ ...styles.compactTh, padding: '8px' }}>Work Task</th>
                            </tr>
                        </thead>
                        <tbody>
                            {tasks.map(t => (
                                <tr key={t.id}>
                                    <td style={{ ...styles.compactTd, padding: '8px', textAlign: 'center' }}>
                                        <input type="checkbox" checked={selected.has(t.id)} onChange={() => toggleTask(t.id)} />
                                    </td>
                                    <td style={{ ...styles.compactTd, padding: '8px' }}>{t.name}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <button onClick={handleSave} style={{ ...styles.smallButton, padding: '8px 20px' }}>Save</button>
                </>
            )}
        </div>
    );
}
