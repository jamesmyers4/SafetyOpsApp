import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import NavBar from '../components/NavBar';

interface Appointment {
    id: number;
    date: string;
    personName: string;
    personId: number;
}

export default function OMSSEditPage() {
    const navigate = useNavigate();
    const [search, setSearch] = useState('');
    const [results, setResults] = useState<Appointment[] | null>(null);
    const [searched, setSearched] = useState(false);
    const [selectedId, setSelectedId] = useState<number | null>(null);
    const [frameKey, setFrameKey] = useState(0);
    const [successMsg, setSuccessMsg] = useState('');

    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };

    const handleMessage = useCallback((event: MessageEvent) => {
        if (event.data?.type === 'omssUpdated') {
            setSuccessMsg('Record updated successfully');
        } else if (event.data?.type === 'omssEditCancelled') {
            setSelectedId(null);
        }
    }, []);

    useEffect(() => {
        window.addEventListener('message', handleMessage);
        return () => window.removeEventListener('message', handleMessage);
    }, [handleMessage]);

    async function handleSearch() {
        try {
            const appts = await api.getOmssAppointments(search);
            setResults(appts);
            setSearched(true);
            setSelectedId(null);
            setSuccessMsg('');
        } catch (e) {
            console.error(e);
        }
    }

    function selectRecord(id: number) {
        setSelectedId(id);
        setFrameKey(k => k + 1);
        setSuccessMsg('');
    }

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/omss'); }} style={navLink}>
                    Medical Surveillance (OMSS)
                </a>
            } />
            <div style={{ padding: '20px 30px' }}>
                <h2 style={{ color: '#1a2744', marginBottom: '16px' }}>Search / Edit OMSS Records</h2>

                <div style={{ marginBottom: '16px', display: 'flex', gap: '8px' }}>
                    <input
                        type="text"
                        placeholder="Search by name, date, or ID..."
                        aria-label="Search OMSS records"
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                        onKeyDown={e => { if (e.key === 'Enter') handleSearch(); }}
                        style={{ padding: '10px', width: '360px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }}
                    />
                    <button onClick={handleSearch}
                        style={{ padding: '10px 24px', background: '#1a2744', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '14px' }}>
                        Search
                    </button>
                </div>

                {successMsg && (
                    <div style={{ background: '#d4edda', color: '#155724', padding: '12px 20px', borderRadius: '4px', marginBottom: '16px', fontSize: '14px', fontWeight: 'bold' }}>
                        {successMsg}
                    </div>
                )}

                {searched && results !== null && results.length === 0 && (
                    <p style={{ color: '#666' }}>No results found. No appointments match your search.</p>
                )}

                {results !== null && results.length > 0 && (
                    <table style={{ width: '100%', borderCollapse: 'collapse', background: 'white', borderRadius: '8px', overflow: 'hidden', boxShadow: '0 2px 8px rgba(0,0,0,0.08)', marginBottom: '20px' }}>
                        <thead>
                            <tr>
                                <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>ID</th>
                                <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>Person</th>
                                <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>Date</th>
                                <th style={{ background: '#1a2744', color: 'white', padding: '12px 16px', textAlign: 'left' }}>Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            {results.map(r => (
                                <tr key={r.id}>
                                    <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{r.id}</td>
                                    <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{r.personName}</td>
                                    <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>{r.date}</td>
                                    <td style={{ padding: '12px 16px', borderBottom: '1px solid #eee' }}>
                                        <a href="#" onClick={e => { e.preventDefault(); selectRecord(r.id); }}
                                            style={{ color: '#1a2744', fontWeight: 'bold', textDecoration: 'underline', cursor: 'pointer' }}>
                                            Edit
                                        </a>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}

                {selectedId !== null && (
                    <iframe
                        key={frameKey}
                        id="frameEdit"
                        name="frameEdit"
                        src={`/n/safetyops/omss/edit-frame?id=${selectedId}`}
                        style={{ width: '100%', height: '60vh', border: '1px solid #ddd', borderRadius: '4px', background: 'white' }}
                        title="OMSS Edit Frame"
                    />
                )}
            </div>
        </div>
    );
}
