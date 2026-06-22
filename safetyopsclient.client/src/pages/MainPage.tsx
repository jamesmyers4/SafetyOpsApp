import NavBar from '../components/NavBar';

export default function MainPage() {
    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar />
            <div style={{ padding: '60px', textAlign: 'center' }}>
                <h2 style={{ color: '#1a2744' }}>Welcome to SAFETYOPS</h2>
                <p style={{ color: '#555' }}>Select a module from the navigation bar to get started.</p>
            </div>
        </div>
    );
}
