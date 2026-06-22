import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import NavBar from '../components/NavBar';

interface User {
    id: number;
    firstName: string;
    lastName: string;
    department: string;
    employeeCategory: string;
}

export default function EditUserSearchPage() {
    const navigate = useNavigate();
    const [search, setSearch] = useState('');
    const [users, setUsers] = useState<User[] | null>(null);
    const [searched, setSearched] = useState(false);

    async function handleSearch() {
        try {
            const results = await api.getUsers(search);
            setUsers(results);
            setSearched(true);
        } catch (e) {
            console.error(e);
        }
    }

    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/personnel'); }} style={navLink}>
                    Personnel Administration
                </a>
            } />
            <div style={{ padding: '40px 60px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '30px' }}>Edit / Search User</h2>
                <div style={{ marginBottom: '24px', display: 'flex', gap: '8px', alignItems: 'center' }}>
                    <input
                        type="text"
                        placeholder="Search users by name or department..."
                        aria-label="Search users"
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                        onKeyDown={e => { if (e.key === 'Enter') handleSearch(); }}
                        style={{ padding: '10px', width: '380px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }}
                    />
                    <button
                        onClick={handleSearch}
                        style={{ padding: '10px 24px', background: '#1a2744', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '14px' }}
                    >
                        Search
                    </button>
                </div>

                {searched && users !== null && users.length === 0 && (
                    <p style={{ color: '#666' }}>No results found for your search.</p>
                )}

                {users !== null && users.length > 0 && (
                    <table style={{ width: '100%', borderCollapse: 'collapse', background: 'white', borderRadius: '8px', overflow: 'hidden', boxShadow: '0 2px 8px rgba(0,0,0,0.1)' }}>
                        <thead>
                            <tr>
                                <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>Name</th>
                                <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>Department</th>
                                <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {users.map(user => (
                                <tr key={user.id}>
                                    <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{user.firstName} {user.lastName}</td>
                                    <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{user.department}</td>
                                    <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>
                                        <a
                                            href="#"
                                            onClick={e => { e.preventDefault(); navigate(`/n/safetyops/personnel/edit/${user.id}`); }}
                                            style={{ color: '#1a2744', fontWeight: 'bold', textDecoration: 'underline', cursor: 'pointer' }}
                                        >
                                            Edit
                                        </a>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>
        </div>
    );
}
