import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';

export default function LoginPage() {
    const navigate = useNavigate();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState<string | null>(null);

    async function handleLogin() {
        try {
            await api.login(username, password);
            navigate('/n/esams/main');
        } catch (e: unknown) {
            setError(e instanceof Error ? e.message : 'Login failed');
        }
    }

    return (
        <div style={{ background: '#1a2744', display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', margin: 0 }}>
            <div style={{ background: 'white', padding: '60px', borderRadius: '8px', width: '400px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '24px' }}>Sign In</h2>
                {error && <div style={{ color: 'red', marginBottom: '16px', fontSize: '14px' }}>{error}</div>}
                <label style={{ display: 'block', marginBottom: '6px', fontWeight: 'bold', color: '#333' }}>Username</label>
                <input
                    type="text"
                    id="Username"
                    aria-label="Username"
                    value={username}
                    onChange={e => setUsername(e.target.value)}
                    onKeyDown={e => { if (e.key === 'Enter') handleLogin(); }}
                    style={{ width: '100%', padding: '10px', marginBottom: '20px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px', boxSizing: 'border-box' }}
                />
                <label style={{ display: 'block', marginBottom: '6px', fontWeight: 'bold', color: '#333' }}>Password</label>
                <input
                    type="password"
                    id="Password"
                    aria-label="Password"
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                    style={{ width: '100%', padding: '10px', marginBottom: '20px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px', boxSizing: 'border-box' }}
                />
                <button
                    onClick={handleLogin}
                    style={{ background: '#1a2744', color: 'white', border: 'none', padding: '12px 28px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer', width: '100%' }}
                >
                    Login
                </button>
            </div>
        </div>
    );
}