import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { api } from '../services/api';
import NavBar from '../components/NavBar';

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
            <div role="dialog" style={{ position: 'relative', background: 'white', borderRadius: '8px', padding: '30px', width: '500px', maxHeight: '400px', overflowY: 'auto' }}>
                <h4 style={{ marginTop: 0, color: '#1a2744' }}>{title}</h4>
                <button onClick={onClose} style={{ position: 'absolute', top: '12px', right: '16px', background: 'none', border: 'none', fontSize: '20px', color: 'red', cursor: 'pointer', fontWeight: 'bold' }}>&#10005;</button>
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
                                    <input type="checkbox" checked={selected === opt} onChange={() => setSelected(selected === opt ? '' : opt)} />
                                </td>
                                <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>{opt}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                <button onClick={() => { onSave(selected); onClose(); }}
                    style={{ background: '#1a2744', color: 'white', border: 'none', padding: '10px 24px', borderRadius: '4px', cursor: 'pointer', marginTop: '16px' }}>
                    Save
                </button>
            </div>
        </div>
    );
}

function GenderCombobox({ value, onChange }: { value: string; onChange: (v: string) => void }) {
    const [open, setOpen] = useState(false);
    const options = ['Male', 'Female', 'Non-binary', 'Prefer not to say'];
    return (
        <div style={{ position: 'relative', display: 'inline-block', width: '374px' }}>
            <div
                role="combobox"
                aria-label="Gender"
                aria-expanded={open}
                tabIndex={0}
                onClick={() => setOpen(o => !o)}
                onKeyDown={e => { if (e.key === 'Enter' || e.key === ' ') setOpen(o => !o); }}
                style={{ padding: '10px', border: '1px solid #ccc', borderRadius: '4px', cursor: 'pointer', background: 'white', fontSize: '14px', userSelect: 'none' }}
            >
                {value || 'Select Gender'}
            </div>
            {open && (
                <ul role="listbox" aria-label="Gender"
                    style={{ position: 'absolute', top: '100%', left: 0, width: '100%', background: 'white', border: '1px solid #ccc', borderRadius: '4px', zIndex: 100, listStyle: 'none', margin: 0, padding: 0, boxShadow: '0 4px 8px rgba(0,0,0,0.1)' }}>
                    {options.map(opt => (
                        <li key={opt} role="option" aria-selected={value === opt}
                            onClick={() => { onChange(opt); setOpen(false); }}
                            style={{ padding: '10px', cursor: 'pointer', background: value === opt ? '#e8ecf7' : 'white' }}>
                            {opt}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

export default function EditUserFormPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [dialog, setDialog] = useState<string | null>(null);
    const [successMsg, setSuccessMsg] = useState('');
    const [errors, setErrors] = useState<string[]>([]);
    const [form, setForm] = useState({
        firstName: '', lastName: '', middleName: '', gender: '',
        department: '', employeeCategory: '', subscription: '', employeeNumber: '',
    });

    useEffect(() => {
        if (id) {
            api.getUser(Number(id)).then(u => setForm({
                firstName: u.firstName, lastName: u.lastName, middleName: u.middleName,
                gender: u.gender, department: u.department, employeeCategory: u.employeeCategory,
                subscription: u.subscription, employeeNumber: u.employeeNumber,
            })).catch(console.error);
        }
    }, [id]);

    function setField(field: string, value: string) {
        setForm(prev => ({ ...prev, [field]: value }));
    }

    async function handleUpdate() {
        const errs: string[] = [];
        if (!form.firstName.trim()) errs.push('First Name is required');
        if (!form.lastName.trim()) errs.push('Last Name is required');
        setErrors(errs);
        if (errs.length > 0) return;
        try {
            await api.updateUser(Number(id), form);
            setSuccessMsg('User updated successfully');
        } catch (e: unknown) {
            setErrors([e instanceof Error ? e.message : 'Update failed']);
        }
    }

    const navLink: React.CSSProperties = { color: '#aac4ff', textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' };
    const labelStyle: React.CSSProperties = { display: 'block', marginBottom: '6px', fontWeight: 'bold', color: '#333' };
    const inputStyle: React.CSSProperties = { width: '350px', padding: '10px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' };
    const selectBtnStyle: React.CSSProperties = { background: '#1a2744', color: 'white', border: 'none', padding: '8px 16px', borderRadius: '4px', cursor: 'pointer', marginLeft: '10px' };

    return (
        <div style={{ background: '#f4f6f9', minHeight: '100vh', margin: 0 }}>
            <NavBar extra={
                <a href="#" onClick={e => { e.preventDefault(); navigate('/n/safetyops/personnel'); }} style={navLink}>
                    Personnel Administration
                </a>
            } />
            <div style={{ padding: '40px 60px' }}>
                <h3 style={{ color: '#1a2744', marginBottom: '30px' }}>Edit User</h3>

                {errors.length > 0 && (
                    <div style={{ color: 'red', marginBottom: '16px', fontSize: '14px' }}>
                        {errors.map((e, i) => <div key={i}>{e}</div>)}
                    </div>
                )}
                {successMsg && (
                    <div style={{ color: 'green', marginBottom: '16px', fontSize: '14px', fontWeight: 'bold' }}>
                        {successMsg}
                    </div>
                )}

                <div title="Select a Department" style={{ marginBottom: '20px' }}>
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
                    <GenderCombobox value={form.gender} onChange={v => setField('gender', v)} />
                </div>

                <div title="Select an Employee Category" style={{ marginBottom: '20px' }}>
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
                </div>

                <div style={{ display: 'flex', gap: '12px', marginTop: '20px' }}>
                    <button onClick={handleUpdate}
                        style={{ background: '#1a2744', color: 'white', border: 'none', padding: '12px 32px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer' }}>
                        Update
                    </button>
                    <button onClick={() => navigate('/n/safetyops/personnel/edit')}
                        style={{ background: '#555', color: 'white', border: 'none', padding: '12px 32px', fontSize: '15px', borderRadius: '4px', cursor: 'pointer' }}>
                        Cancel
                    </button>
                </div>
            </div>

            {dialog === 'department' && (
                <SelectDialog title="Select a Department"
                    options={['Engineering', 'Operations', 'Human Resources', 'Finance', 'Safety']}
                    onSave={val => setField('department', val)} onClose={() => setDialog(null)} />
            )}
            {dialog === 'subscription' && (
                <SelectDialog title="Subscriptions"
                    options={['Basic', 'Standard', 'Premium']}
                    onSave={val => setField('subscription', val)} onClose={() => setDialog(null)} />
            )}
            {dialog === 'category' && (
                <SelectDialog title="Select an Employee Category"
                    options={['Full Time', 'Part Time', 'Contractor', 'Intern']}
                    onSave={val => setField('employeeCategory', val)} onClose={() => setDialog(null)} />
            )}
        </div>
    );
}
