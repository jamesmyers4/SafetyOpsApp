import { useState } from 'react';
import { api } from '../services/api';

interface WorkTask {
    id: string;
    name: string;
    stressors: { stressorId: string; stressorName: string }[];
    examTypeOptions: string[];
}

export default function WorkTaskPickerPage() {
    const [tasks, setTasks] = useState<WorkTask[]>([]);
    const [selected, setSelected] = useState<Set<string>>(new Set());
    const [searched, setSearched] = useState(false);

    async function handleSearch() {
        try {
            const results = await api.getOmssWorkTasks();
            setTasks(results);
            setSearched(true);
        } catch (e) {
            console.error(e);
        }
    }

    function toggleTask(id: string) {
        setSelected(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id); else next.add(id);
            return next;
        });
    }

    function handleSave() {
        const selectedTasks = tasks
            .filter(t => selected.has(t.id))
            .flatMap(t => t.stressors.map(s => ({
                stressorId: s.stressorId,
                stressorName: s.stressorName,
                examTypeOptions: t.examTypeOptions,
            })));
        window.parent.postMessage({ type: 'workTasksSelected', tasks: selectedTasks }, '*');
    }

    return (
        <div style={{ fontFamily: 'Arial, sans-serif', padding: '16px', background: 'white', minHeight: '100%' }}>
            <h4 style={{ color: '#1a2744', marginTop: 0 }}>Select Work Tasks</h4>
            <button onClick={handleSearch}
                style={{ background: '#1a2744', color: 'white', border: 'none', padding: '8px 20px', borderRadius: '4px', cursor: 'pointer', marginBottom: '12px' }}>
                Search
            </button>

            {searched && tasks.length === 0 && <p style={{ color: '#666' }}>No work tasks found.</p>}

            {tasks.length > 0 && (
                <>
                    <table style={{ width: '100%', borderCollapse: 'collapse', marginBottom: '12px' }}>
                        <thead>
                            <tr>
                                <th style={{ background: '#1a2744', color: 'white', padding: '8px', textAlign: 'left', width: '40px' }}>Select</th>
                                <th style={{ background: '#1a2744', color: 'white', padding: '8px', textAlign: 'left' }}>Work Task</th>
                            </tr>
                        </thead>
                        <tbody>
                            {tasks.map(t => (
                                <tr key={t.id}>
                                    <td style={{ padding: '8px', borderBottom: '1px solid #eee', textAlign: 'center' }}>
                                        <input type="checkbox" checked={selected.has(t.id)} onChange={() => toggleTask(t.id)} />
                                    </td>
                                    <td style={{ padding: '8px', borderBottom: '1px solid #eee' }}>{t.name}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <button onClick={handleSave}
                        style={{ background: '#1a2744', color: 'white', border: 'none', padding: '8px 20px', borderRadius: '4px', cursor: 'pointer' }}>
                        Save
                    </button>
                </>
            )}
        </div>
    );
}
