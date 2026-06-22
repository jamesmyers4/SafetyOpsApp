import { useState } from 'react';
import { api } from '../services/api';

interface Person {
    id: number;
    name: string;
}

export default function PersonPickerPage() {
    const [persons, setPersons] = useState<Person[]>([]);
    const [searched, setSearched] = useState(false);

    async function handleSearch() {
        try {
            const results = await api.getOmssPersons();
            setPersons(results);
            setSearched(true);
        } catch (e) {
            console.error(e);
        }
    }

    function selectPerson(person: Person) {
        if (window.opener) {
            window.opener.postMessage({ type: 'personSelected', id: person.id, name: person.name }, '*');
        }
        window.close();
    }

    return (
        <div style={{ fontFamily: 'Arial, sans-serif', padding: '20px', background: 'white', minHeight: '100vh' }}>
            <h3 style={{ color: '#1a2744', marginTop: 0 }}>Select Person Evaluated</h3>
            <button onClick={handleSearch}
                style={{ background: '#1a2744', color: 'white', border: 'none', padding: '8px 20px', borderRadius: '4px', cursor: 'pointer', marginBottom: '16px' }}>
                Search
            </button>

            {searched && persons.length === 0 && <p style={{ color: '#666' }}>No persons found.</p>}

            {persons.length > 0 && (
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead>
                        <tr>
                            <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>Name</th>
                        </tr>
                    </thead>
                    <tbody>
                        {persons.map(p => (
                            <tr key={p.id}>
                                <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>
                                    <a href="#" onClick={e => { e.preventDefault(); selectPerson(p); }}
                                        style={{ color: '#1a2744', textDecoration: 'underline', cursor: 'pointer' }}>
                                        {p.name}
                                    </a>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}
