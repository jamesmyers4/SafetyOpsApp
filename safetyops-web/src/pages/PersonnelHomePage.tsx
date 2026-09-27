import { useEffect, useState } from 'react';
import { api } from '../services/api';
import type { Paged, Person } from '../types/api';
import LoadStatus from '../components/LoadStatus';
import { errorMessage } from '../services/errors';
import NavLink from '../components/NavLink';
import PageLayout from '../components/PageLayout';
import Pager from '../components/Pager';
import { styles } from '../styles/theme';

const PAGE_SIZE = 25;

export default function PersonnelHomePage() {
    const [data, setData] = useState<Paged<Person> | null>(null);
    const [page, setPage] = useState(1);
    const [search, setSearch] = useState('');
    const [appliedSearch, setAppliedSearch] = useState('');
    const [reloadKey, setReloadKey] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        api.listPeople({ search: appliedSearch, page, pageSize: PAGE_SIZE })
            .then(result => { if (!cancelled) { setData(result); setError(null); } })
            .catch((e: unknown) => { if (!cancelled) setError(errorMessage(e, 'Failed to load users')); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [appliedSearch, page, reloadKey]);

    function handleSearch() {
        setLoading(true);
        setPage(1);
        setAppliedSearch(search.trim());
    }

    function goToPage(next: number) {
        setLoading(true);
        setPage(next);
    }

    async function handleDelete(user: Person) {
        if (!window.confirm(`Delete ${user.firstName} ${user.lastName}?`)) return;
        try {
            await api.deleteUser(user.id);
            // Step back a page if this removed the last row on it; otherwise refresh the current page.
            if (data && data.items.length === 1 && page > 1) goToPage(page - 1);
            else setReloadKey(k => k + 1);
        } catch (e: unknown) {
            setError(errorMessage(e, 'Delete failed'));
        }
    }

    return (
        <PageLayout section={{ label: 'Personnel', to: '/personnel' }}>
            <h2 style={styles.heading}>Personnel</h2>
            <div style={{ ...styles.buttonRow, marginBottom: '20px' }}>
                <NavLink to="/personnel/create" role="link" style={styles.buttonLink}>Add New User</NavLink>
                <NavLink to="/personnel/edit" role="link" style={styles.buttonLink}>Edit/Search User</NavLink>
                <NavLink to="/personnel/access-levels" role="link" style={styles.buttonLink}>Access Levels</NavLink>
            </div>
            <div style={styles.field}>
                <input type="text" placeholder="Search users..." aria-label="Filter users"
                    value={search} onChange={e => setSearch(e.target.value)}
                    onKeyDown={e => { if (e.key === 'Enter') handleSearch(); }}
                    style={{ ...styles.input, width: '300px' }} />
                <button onClick={handleSearch} style={{ ...styles.smallButton, padding: '10px 20px', marginLeft: '8px' }}>Search</button>
            </div>
            <LoadStatus loading={loading && !data} error={error} />
            <table style={styles.cardTable}>
                <thead>
                    <tr>
                        <th style={styles.th}>Name</th>
                        <th style={styles.th}>Department</th>
                        <th style={styles.th}>Category</th>
                        <th style={styles.th}>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {data?.items.map(user => (
                        <tr key={user.id}>
                            <td style={styles.td}>{user.firstName} {user.lastName}</td>
                            <td style={styles.td}>{user.department}</td>
                            <td style={styles.td}>{user.employeeCategory}</td>
                            <td style={styles.td}>
                                <button onClick={() => handleDelete(user)} style={styles.smallDangerButton}>Delete</button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
            {data && <Pager data={data} noun={['person', 'people']} disabled={loading} onPage={goToPage} />}
        </PageLayout>
    );
}
