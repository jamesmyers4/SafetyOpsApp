import { useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { api } from '../services/api';

interface User {
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

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <div style={{ background: '#1a2744', padding: '14px 30px', color: 'white', fontSize: '18px', fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: '30px' }}>
                ESAMS
                <a onClick={() => navigate('/n/esams/main')} style={{ color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' }}>Modules</a>
                <a onClick={() => navigate('/n/esams/personnel')} style={{ color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' }}>Personnel Administration</a>
            </div>
            <div style={{ padding: '40px 60px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '30px' }}>Personnel Administration</h2>
                <div style={{ marginBottom: '20px' }}>
                    <input type="text" placeholder="Search users..." style={{ padding: '10px', width: '300px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }} />
                    <button style={{ padding: '10px 20px', background: '#1a2744', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', marginLeft: '8px' }}>Search</button>
                </div>
                <div style={{ marginBottom: '30px' }}>
                    <button
                        onClick={() => navigate('/n/esams/personnel/create')}
                        style={{ background: '#1a2744', color: 'white', border: 'none', padding: '10px 24px', borderRadius: '4px', cursor: 'pointer', fontSize: '15px' }}
                    >
                        Add New User
                    </button>
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
                        {users.map((user, i) => (
                            <tr key={i}>
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