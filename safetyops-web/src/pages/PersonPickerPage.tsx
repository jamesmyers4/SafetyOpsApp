import { useCallback } from 'react';
import { api } from '../services/api';
import { postToOpener } from '../services/messaging';
import { useSearch } from '../hooks/useSearch';
import type { PersonOption } from '../types/api';
import LoadStatus from '../components/LoadStatus';
import { styles } from '../styles/theme';

/** Popup window opened by the appointment frames; sends the chosen person back and closes. */
export default function PersonPickerPage() {
    const { results: persons, loading, error, run } = useSearch(useCallback(() => api.getPersonOptions(), []));

    function selectPerson(person: PersonOption) {
        postToOpener({ type: 'personSelected', id: person.id, name: person.name });
        window.close();
    }

    return (
        <div style={styles.frame}>
            <h3 style={styles.frameHeading}>Select Person Evaluated</h3>
            <button onClick={run} style={{ ...styles.smallButton, padding: '8px 20px', marginBottom: '16px' }}>Search</button>

            <LoadStatus loading={loading} error={error} />
            {!loading && persons?.length === 0 && <p style={styles.emptyText}>No persons found.</p>}

            {persons && persons.length > 0 && (
                <table style={styles.table}>
                    <thead>
                        <tr>
                            <th style={styles.compactTh}>Name</th>
                        </tr>
                    </thead>
                    <tbody>
                        {persons.map(p => (
                            <tr key={p.id}>
                                <td style={styles.compactTd}>
                                    <a href="#" onClick={e => { e.preventDefault(); selectPerson(p); }} style={styles.textLink}>{p.name}</a>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}
