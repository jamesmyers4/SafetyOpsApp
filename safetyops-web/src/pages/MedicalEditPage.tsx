import { useCallback, useState } from 'react';
import { api } from '../services/api';
import { useAppMessages } from '../services/messaging';
import { useSearch } from '../hooks/useSearch';
import LoadStatus from '../components/LoadStatus';
import PageLayout from '../components/PageLayout';
import { styles } from '../styles/theme';

/** Search appointments, then edit the selected one in an iframe below the results. */
export default function MedicalEditPage() {
    const [search, setSearch] = useState('');
    const { results, loading, error, run } = useSearch(useCallback(() => api.getAppointments(search), [search]));
    const [selectedId, setSelectedId] = useState<number | null>(null);
    const [frameKey, setFrameKey] = useState(0);
    const [successMsg, setSuccessMsg] = useState('');

    useAppMessages(message => {
        if (message.type === 'appointmentUpdated') setSuccessMsg('Record updated successfully');
        else if (message.type === 'appointmentEditCancelled') setSelectedId(null);
    });

    async function handleSearch() {
        setSelectedId(null);
        setSuccessMsg('');
        await run();
    }

    function selectRecord(id: number) {
        setSelectedId(id);
        setFrameKey(k => k + 1);
        setSuccessMsg('');
    }

    return (
        <PageLayout section={{ label: 'Medical Surveillance', to: '/medical-surveillance' }} variant="shell">
            <h2 style={{ ...styles.heading, marginBottom: '16px' }}>Search / Edit Medical Surveillance Records</h2>

            <div style={{ ...styles.frameField, display: 'flex', gap: '8px' }}>
                <input
                    type="text"
                    placeholder="Search by name, date, or ID..."
                    aria-label="Search medical surveillance records"
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                    onKeyDown={e => { if (e.key === 'Enter') handleSearch(); }}
                    style={styles.searchInput}
                />
                <button onClick={handleSearch} style={{ ...styles.smallButton, padding: '10px 24px', fontSize: '14px' }}>Search</button>
            </div>

            {successMsg && <div role="status" style={{ ...styles.alertSuccess, fontWeight: 'bold' }}>{successMsg}</div>}

            <LoadStatus loading={loading} error={error} />
            {!loading && results?.length === 0 && (
                <p style={styles.emptyText}>No results found. No appointments match your search.</p>
            )}

            {results && results.length > 0 && (
                <table style={{ ...styles.cardTable, boxShadow: '0 2px 8px rgba(0,0,0,0.08)', marginBottom: '20px' }}>
                    <thead>
                        <tr>
                            <th style={styles.th}>ID</th>
                            <th style={styles.th}>Person</th>
                            <th style={styles.th}>Date</th>
                            <th style={styles.th}>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {results.map(r => (
                            <tr key={r.id}>
                                <td style={styles.td}>{r.id}</td>
                                <td style={styles.td}>{r.personName}</td>
                                <td style={styles.td}>{r.date}</td>
                                <td style={styles.td}>
                                    <a href="#" onClick={e => { e.preventDefault(); selectRecord(r.id); }} style={styles.boldLink}>Edit</a>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

            {selectedId !== null && (
                <iframe
                    key={frameKey}
                    id="edit-frame"
                    name="edit-frame"
                    src={`/medical-surveillance/edit-frame?id=${selectedId}`}
                    style={{ width: '100%', height: '60vh', border: '1px solid #ddd', borderRadius: '4px', background: 'white' }}
                    title="Medical Surveillance Edit Frame"
                />
            )}
        </PageLayout>
    );
}
