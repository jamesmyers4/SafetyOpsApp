import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';

interface DialogProps {
    title: string;
    options: string[];
    onSave: (value: string) => void;
    onClose: () => void;
}

function SelectDialog({ title, options, onSave, onClose }: DialogProps) {
    const [selected, setSelected] = useState('');
    return (
        <div onClick={e => { if (e.target === e.currentTarget) onClose(); }}
            style={{ position: 'fixed', top: 0, left: 0, width: '100%', height: '100%', background: 'rgba(0,0,0,0.5)', display: 'flex', justifyContent: 'center', alignItems: 'center', zIndex: 999 }}>
            <div style={{ position: 'relative', background: 'white', borderRadius: '8px', padding: '30px', width: '500px', maxHeight: '400px', overflowY: 'auto' }}>
                <h4 style={{ marginTop: 0, color: '#1a2744' }}>{title}</h4>
                <button onClick={onClose} style={{ position: 'absolute', top: '12px', right: '16px', background: 'none', border: 'none', fontSize: '20px', color: 'red', cursor: 'pointer', fontWeight: 'bold', zIndex: 1000 }}>&#10005;</button>
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead>
                        <tr>
                            <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>Select</th>
                            <th style={{ background: '#1a2744', color: 'white', padding: '10px', textAlign: 'left' }}>Option</th>
                        </tr>
                    </thead>
                    <tbody>
                        {options.map(opt => (
                            <tr key={opt}>
                                <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>
                                    <input
                                        type="checkbox"
                                        checked={selected === opt}
                                        onChange={() => setSelected(selected === opt ? '' : opt)}
                                    />
                                </td>
                                <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>{opt}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                <button
                    onClick={() => { onSave(selected); onClose(); }}
                    style={{ background: '#1a2744', color: 'white', border: 'none', padding: '10px 24px', borderRadius: '4px', cursor: 'pointer', marginTop: '16px' }}
                >
                    Save
                </button>

            </div>
        </div>
    );
}

export default function AddUserPage() {
    const navigate = useNavigate();
    const [dialog, setDialog] = useState<string | null>(null);
    const [form, setForm] = useState({
        firstName: '',
        lastName: '',
        middleName: '',
        gender: '',
        department: '',
        employeeCategory: '',
        subscription: '',
        employeeNumber: '',
    });

    function setField(field: string, value: string) {
        setForm(prev => ({ ...prev, [field]: value }));
    }

    function generateNumber() {
        setField('employeeNumber', String(Math.floor(Math.random() * 9000000 + 1000000)));
    }

    async function handleSubmit() {
        try {
            await api.addUser(form);
            navigate('/n/safetyops/personnel/success');
        } catch (e) {
            console.error(e);
        }
    }

    const navStyle = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' } as React.CSSProperties;
    const labelStyle = { display: 'block', marginBottom: '6px', fontWeight: 'bold', color: '#333' } as React.CSSProperties;
    const inputStyle = { width: '350px', padding: '10px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' } as React.CSSProperties;
    const selectBtnStyle = { background: '#1a2744', color: 'white', border: 'none', padding: '8px 16px', borderRadius: '4px', cursor: 'pointer', marginLeft: '10px' } as React.CSSProperties;

    return (
        <div onKeyDown={e => { if (e.key === 'Enter' && !dialog) handleSubmit(); }}
             style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
             <div style={{ background: '#1a2744', padding: '14px 30px', color: 'white', fontSize: '18px', fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: '30px' }}>
                SAFETOPS
                <a onClick={() => navigate('/n/safetyops/main')} style={navStyle}>Modules</a>
                <a onClick={() => navigate('/n/safetyops/personnel')} style={navStyle}>Personnel Administration</a>
            </div>
            <div style={{ padding: '40px 60px' }}>
                <h3 style={{ color: '#1a2744', marginBottom: '30px' }}>Add New User</h3>

                <div style={{ marginBottom: '20px' }}>
                    <label style={labelStyle}>Department</label>
                    <span style={{ marginRight: '10px' }}>{form.department || 'None selected'}</span>
                    <button style={selectBtnStyle} onClick={() => setDialog('department')}>Open Select List</button>
                </div>

                <div style={{ marginBottom: '20px' }}>
                    <label style={labelStyle}>Subscriptions</label>
                    <span style={{ marginRight: '10px' }}>{form.subscription || 'None selected'}</span>
                    <button style={selectBtnStyle} onClick={() => setDialog('subscription')}>Subscriptions</button>
                </div>

                <div style={{ marginBottom: '20px' }}>
                    <label style={labelStyle}>Gender</label>
                    <select
                        aria-label="Gender"
                        value={form.gender}
                        onChange={e => setField('gender', e.target.value)}
                        style={{ width: '374px', padding: '10px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }}
                    >
                        <option value="">Select Gender</option>
                        <option value="Male">Male</option>
                        <option value="Female">Female</option>
                        <option value="Non-binary">Non-binary</option>
                        <option value="Prefer not to say">Prefer not to say</option>
                    </select>
                </div>

                <div style={{ marginBottom: '20px' }}>
                    <label style={labelStyle}>Employee Category</label>
                    <span style={{ marginRight: '10px' }}>{form.employeeCategory || 'None selected'}</span>
                    <button style={selectBtnStyle} onClick={() => setDialog('category')}>Open Select List</button>
                </div>

                <div style={{ marginBottom: '20px' }}>
                    <label style={labelStyle}>First Name</label>
                    <input aria-label="First Name" type="text" value={form.firstName} onChange={e => setField('firstName', e.target.value)} style={inputStyle} />
                </div>

                <div style={{ marginBottom: '20px' }}>
                    <label style={labelStyle}>Last Name</label>
                    <input aria-label="Last Name" type="text" value={form.lastName} onChange={e => setField('lastName', e.target.value)} style={inputStyle} />
                </div>

                <div style={{ marginBottom: '20px' }}>
                    <label style={labelStyle}>Middle Name</label>
                    <input aria-label="Middle Name" type="text" value={form.middleName} onChange={e => setField('middleName', e.target.value)} style={inputStyle} />
                </div>

                <div style={{ marginBottom: '20px' }}>
                    <label style={labelStyle}>Employee Number</label>
                    <input type="text" value={form.employeeNumber} readOnly style={inputStyle} />
                    <button style={{ ...selectBtnStyle, background: '#555' }} onClick={generateNumber}>Generate Random Number</button>
                </div>

                <button
                    onClick={handleSubmit}
                    style={{ background: '#1a2744', color: 'white', border: 'none', padding: '12px 32px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer', marginTop: '10px' }}
                >
                    Add User
                </button>
            </div>

            {dialog === 'department' && (
                <SelectDialog
                    title="Select a Department"
                    options={['Engineering', 'Operations', 'Human Resources', 'Finance', 'Safety']}
                    onSave={val => setField('department', val)}
                    onClose={() => setDialog(null)}
                />
            )}
            {dialog === 'subscription' && (
                <SelectDialog
                    title="Subscriptions"
                    options={['Basic', 'Standard', 'Premium']}
                    onSave={val => setField('subscription', val)}
                    onClose={() => setDialog(null)}
                />
            )}
            {dialog === 'category' && (
                <SelectDialog
                    title="Select an Employee Category"
                    options={['Full Time', 'Part Time', 'Contractor', 'Intern']}
                    onSave={val => setField('employeeCategory', val)}
                    onClose={() => setDialog(null)}
                />
            )}
        </div>
    );
}