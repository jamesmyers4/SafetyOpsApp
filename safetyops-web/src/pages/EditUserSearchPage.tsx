import { useCallback, useState } from 'react';
import { api } from '../services/api';
import { useSearch } from '../hooks/useSearch';
import LoadStatus from '../components/LoadStatus';
import NavLink from '../components/NavLink';
import PageLayout from '../components/PageLayout';
import { styles } from '../styles/theme';

export default function EditUserSearchPage() {
    const [search, setSearch] = useState('');
    const { results: users, loading, error, run } = useSearch(useCallback(() => api.getUsers(search), [search]));

    return (
        <PageLayout section={{ label: 'Personnel', to: '/personnel' }}>
            <h2 style={styles.heading}>Edit / Search User</h2>
            <div style={{ marginBottom: '24px', display: 'flex', gap: '8px', alignItems: 'center' }}>
                <input
                    type="text"
                    placeholder="Search users by name or department..."
                    aria-label="Search users"
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                    onKeyDown={e => { if (e.key === 'Enter') run(); }}
                    style={{ ...styles.searchInput, width: '380px' }}
                />
                <button onClick={run} style={{ ...styles.smallButton, padding: '10px 24px', fontSize: '14px' }}>
                    Search
                </button>
            </div>

            <LoadStatus loading={loading} error={error} />

            {!loading && users?.length === 0 && (
                <p style={styles.emptyText}>No results found for your search.</p>
            )}

            {users && users.length > 0 && (
                <table style={styles.cardTable}>
                    <thead>
                        <tr>
                            <th style={styles.th}>Name</th>
                            <th style={styles.th}>Department</th>
                            <th style={styles.th}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map(user => (
                            <tr key={user.id}>
                                <td style={styles.td}>{user.firstName} {user.lastName}</td>
                                <td style={styles.td}>{user.department}</td>
                                <td style={styles.td}>
                                    <NavLink to={`/personnel/edit/${user.id}`} style={styles.boldLink}>Edit</NavLink>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </PageLayout>
    );
}
