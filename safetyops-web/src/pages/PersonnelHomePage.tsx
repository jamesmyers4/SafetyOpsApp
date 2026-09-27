import { useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { api, type Paged, type Person } from '../services/api';
import NavBar from '../components/NavBar';

const PAGE_SIZE = 25;

export default function PersonnelHomePage() {
    const navigate = useNavigate();
    const [data, setData] = useState<Paged<Person> | null>(null);
    const [page, setPage] = useState(1);
    const [search, setSearch] = useState('');
    const [appliedSearch, setAppliedSearch] = useState('');
    const [reloadKey, setReloadKey] = useState(0);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        api.listPeople({ search: appliedSearch, page, pageSize: PAGE_SIZE })
            .then(result => { if (!cancelled) { setData(result); setError(null); } })
            .catch((e: unknown) => { if (!cancelled) setError(e instanceof Error ? e.message : 'Failed to load users'); });
        return () => { cancelled = true; };
    }, [appliedSearch, page, reloadKey]);

    function handleSearch() {
        setPage(1);
        setAppliedSearch(search.trim());
    }

    async function handleDelete(user: Person) {
        if (!window.confirm(`Delete ${user.firstName} ${user.lastName}?`)) return;
        try {
            await api.deleteUser(user.id);
            // Step back a page if this removed the last row on it; otherwise refresh the current page.
            if (data && data.items.length === 1 && page > 1) setPage(p => p - 1);
            else setReloadKey(k => k + 1);
        } catch (e: unknown) {
            setError(e instanceof Error ? e.message : 'Delete failed');
        }
    }

    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/personnel'); }} style={navLink}>
                    Personnel
                </a>
            } />
            <div style={{ padding: '40px 60px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '30px' }}>Personnel</h2>
                <div style={{ marginBottom: '20px', display: 'flex', gap: '12px', flexWrap: 'wrap' }}>
                    <a href="#" role="link" onClick={e => { e.preventDefault(); navigate('/personnel/create'); }}
                        style={{ background: '#1a2744', color: 'white', textDecoration: 'none', padding: '10px 24px', borderRadius: '4px', fontSize: '15px' }}>
                        Add New User
                    </a>
                    <a href="#" role="link" onClick={e => { e.preventDefault(); navigate('/personnel/edit'); }}
                        style={{ background: '#1a2744', color: 'white', textDecoration: 'none', padding: '10px 24px', borderRadius: '4px', fontSize: '15px' }}>
                        Edit/Search User
                    </a>
                    <a href="#" role="link" onClick={e => { e.preventDefault(); navigate('/personnel/access-levels'); }}
                        style={{ background: '#1a2744', color: 'white', textDecoration: 'none', padding: '10px 24px', borderRadius: '4px', fontSize: '15px' }}>
                        Access Levels
                    </a>
                </div>
                {error && <div style={{ color: 'red', marginBottom: '16px', fontSize: '14px' }}>{error}</div>}
                <div style={{ marginBottom: '20px' }}>
                    <input type="text" placeholder="Search users..." aria-label="Filter users"
                        value={search} onChange={e => setSearch(e.target.value)}
                        onKeyDown={e => { if (e.key === 'Enter') handleSearch(); }}
                        style={{ padding: '10px', width: '300px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }} />
                    <button onClick={handleSearch} style={{ padding: '10px 20px', background: '#1a2744', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', marginLeft: '8px' }}>Search</button>
                </div>
                <table style={{ width: '100%', borderCollapse: 'collapse', background: 'white', borderRadius: '8px', overflow: 'hidden', boxShadow: '0 2px 8px rgba(0,0,0,0.1)' }}>
                    <thead>
                        <tr>
                            <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>Name</th>
                            <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>Department</th>
                            <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>Category</th>
                            <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {data?.items.map((user) => (
                            <tr key={user.id}>
                                <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{user.firstName} {user.lastName}</td>
                                <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{user.department}</td>
                                <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{user.employeeCategory}</td>
                                <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>
                                    <button onClick={() => handleDelete(user)} style={{ background: '#cc0000', color: 'white', border: 'none', padding: '6px 14px', borderRadius: '4px', cursor: 'pointer' }}>Delete</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                {data && (
                    <div style={{ marginTop: '16px', display: 'flex', gap: '12px', alignItems: 'center', fontSize: '14px', color: '#555' }}>
                        <button onClick={() => setPage(p => p - 1)} disabled={page <= 1}
                            style={{ padding: '6px 14px', borderRadius: '4px', border: '1px solid #ccc', background: 'white', cursor: page <= 1 ? 'default' : 'pointer' }}>
                            Previous
                        </button>
                        <span>Page {data.totalPages === 0 ? 0 : data.page} of {data.totalPages} ({data.totalCount} {data.totalCount === 1 ? 'person' : 'people'})</span>
                        <button onClick={() => setPage(p => p + 1)} disabled={page >= data.totalPages}
                            style={{ padding: '6px 14px', borderRadius: '4px', border: '1px solid #ccc', background: 'white', cursor: page >= data.totalPages ? 'default' : 'pointer' }}>
                            Next
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}
