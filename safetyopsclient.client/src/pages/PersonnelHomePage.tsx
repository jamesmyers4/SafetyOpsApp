import { useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { api } from '../services/api';
import NavBar from '../components/NavBar';

interface User {
    id: number;
    firstName: string;
    lastName: string;
    department: string;
    employeeCategory: string;
}

export default function PersonnelHomePage() {
    const navigate = useNavigate();
    const [users, setUsers] = useState<User[]>([]);

    useEffect(() => {
        api.getUsers().then(setUsers).catch(console.error);
    }, []);

    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/personnel'); }} style={navLink}>
                    Personnel Administration
                </a>
            } />
            <div style={{ padding: '40px 60px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '30px' }}>Personnel Administration</h2>
                <div style={{ marginBottom: '20px', display: 'flex', gap: '12px', flexWrap: 'wrap' }}>
                    <a href="#" role="link" onClick={e => { e.preventDefault(); navigate('/n/safetyops/personnel/create'); }}
                        style={{ background: '#1a2744', color: 'white', textDecoration: 'none', padding: '10px 24px', borderRadius: '4px', fontSize: '15px' }}>
                        Add New User
                    </a>
                    <a href="#" role="link" onClick={e => { e.preventDefault(); navigate('/n/safetyops/personnel/edit'); }}
                        style={{ background: '#1a2744', color: 'white', textDecoration: 'none', padding: '10px 24px', borderRadius: '4px', fontSize: '15px' }}>
                        Edit/Search User
                    </a>
                    <a href="#" role="link" onClick={e => { e.preventDefault(); navigate('/n/safetyops/personnel/access'); }}
                        style={{ background: '#1a2744', color: 'white', textDecoration: 'none', padding: '10px 24px', borderRadius: '4px', fontSize: '15px' }}>
                        Access Levels
                    </a>
                </div>
                <div style={{ marginBottom: '20px' }}>
                    <input type="text" placeholder="Search users..." style={{ padding: '10px', width: '300px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }} />
                    <button style={{ padding: '10px 20px', background: '#1a2744', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', marginLeft: '8px' }}>Search</button>
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
                        {users.map((user) => (
                            <tr key={user.id}>
                                <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{user.firstName} {user.lastName}</td>
                                <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{user.department}</td>
                                <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{user.employeeCategory}</td>
                                <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>
                                    <button style={{ background: '#cc0000', color: 'white', border: 'none', padding: '6px 14px', borderRadius: '4px', cursor: 'pointer' }}>Delete</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}
