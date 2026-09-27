import { useState, type FormEvent } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { api } from '../services/api';
import { safeReturnUrl } from '../auth/currentUser';
import { errorMessage } from '../services/errors';
import { colors, styles } from '../styles/theme';

const fieldInput = { ...styles.input, width: '100%', marginBottom: '20px', boxSizing: 'border-box' as const };

export default function LoginPage() {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    async function handleLogin(e: FormEvent) {
        e.preventDefault();
        setSubmitting(true);
        try {
            await api.login(username, password);
            navigate(safeReturnUrl(searchParams.get('returnUrl')), { replace: true });
        } catch (err: unknown) {
            setError(errorMessage(err, 'Login failed'));
            setSubmitting(false);
        }
    }

    return (
        <div style={{ background: colors.navy, display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', margin: 0 }}>
            <form onSubmit={handleLogin} style={{ background: 'white', padding: '60px', borderRadius: '8px', width: '400px' }}>
                <h2 style={{ color: colors.navy, marginBottom: '24px' }}>Sign In</h2>
                {error && <div role="alert" style={styles.errorText}>{error}</div>}
                <label htmlFor="username" style={styles.label}>Username</label>
                <input type="text" id="username" aria-label="Username" autoComplete="username"
                    value={username} onChange={e => setUsername(e.target.value)} style={fieldInput} />
                <label htmlFor="password" style={styles.label}>Password</label>
                <input type="password" id="password" aria-label="Password" autoComplete="current-password"
                    value={password} onChange={e => setPassword(e.target.value)} style={fieldInput} />
                <button type="submit" disabled={submitting} style={{ ...styles.primaryButton, padding: '12px 28px', width: '100%' }}>
                    Login
                </button>
            </form>
        </div>
    );
}
