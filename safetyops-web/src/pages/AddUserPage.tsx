import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import type { PersonInput } from '../types/api';
import { errorMessage } from '../services/errors';
import PageLayout from '../components/PageLayout';
import SelectDialog from '../components/SelectDialog';
import { styles } from '../styles/theme';
import { EMPTY_PERSON, GENDERS, PERSON_DIALOGS, type PersonDialog } from './personnelOptions';

const pickerButton = { ...styles.smallButton, marginLeft: '10px' };

export default function AddUserPage() {
    const navigate = useNavigate();
    const [dialog, setDialog] = useState<PersonDialog | null>(null);
    const [form, setForm] = useState<PersonInput>(EMPTY_PERSON);
    const [error, setError] = useState<string | null>(null);

    function setField(field: keyof PersonInput, value: string) {
        setForm(prev => ({ ...prev, [field]: value }));
    }

    function generateNumber() {
        setField('employeeNumber', String(Math.floor(Math.random() * 9000000 + 1000000)));
    }

    async function handleSubmit() {
        try {
            await api.addUser(form);
            navigate('/personnel/success');
        } catch (e: unknown) {
            setError(errorMessage(e, 'Could not add the user'));
        }
    }

    const openDialog = dialog && PERSON_DIALOGS[dialog];

    return (
        <div onKeyDown={e => { if (e.key === 'Enter' && !dialog) handleSubmit(); }}>
            <PageLayout section={{ label: 'Personnel', to: '/personnel' }}>
                <h3 style={styles.heading}>Add New User</h3>
                {error && <div role="alert" style={styles.errorText}>{error}</div>}

                <div style={styles.field}>
                    <label style={styles.label}>Department</label>
                    <span style={{ marginRight: '10px' }}>{form.department || 'None selected'}</span>
                    <button style={pickerButton} onClick={() => setDialog('department')}>Open Select List</button>
                </div>

                <div style={styles.field}>
                    <label style={styles.label}>Subscriptions</label>
                    <span style={{ marginRight: '10px' }}>{form.subscription || 'None selected'}</span>
                    <button style={pickerButton} onClick={() => setDialog('subscription')}>Subscriptions</button>
                </div>

                <div style={styles.field}>
                    <label style={styles.label}>Gender</label>
                    <select aria-label="Gender" value={form.gender} onChange={e => setField('gender', e.target.value)}
                        style={{ ...styles.input, width: '374px' }}>
                        <option value="">Select Gender</option>
                        {GENDERS.map(g => <option key={g} value={g}>{g}</option>)}
                    </select>
                </div>

                <div style={styles.field}>
                    <label style={styles.label}>Employee Category</label>
                    <span style={{ marginRight: '10px' }}>{form.employeeCategory || 'None selected'}</span>
                    <button style={pickerButton} onClick={() => setDialog('category')}>Open Select List</button>
                </div>

                <div style={styles.field}>
                    <label style={styles.label}>First Name</label>
                    <input aria-label="First Name" type="text" value={form.firstName} onChange={e => setField('firstName', e.target.value)} style={styles.input} />
                </div>

                <div style={styles.field}>
                    <label style={styles.label}>Last Name</label>
                    <input aria-label="Last Name" type="text" value={form.lastName} onChange={e => setField('lastName', e.target.value)} style={styles.input} />
                </div>

                <div style={styles.field}>
                    <label style={styles.label}>Middle Name</label>
                    <input aria-label="Middle Name" type="text" value={form.middleName} onChange={e => setField('middleName', e.target.value)} style={styles.input} />
                </div>

                <div style={styles.field}>
                    <label style={styles.label}>Employee Number</label>
                    <input type="text" value={form.employeeNumber} readOnly style={styles.input} />
                    <button style={{ ...pickerButton, background: '#555' }} onClick={generateNumber}>Generate Random Number</button>
                </div>

                <button onClick={handleSubmit} style={{ ...styles.primaryButton, padding: '12px 32px', marginTop: '10px' }}>
                    Add User
                </button>
            </PageLayout>

            {openDialog && (
                <SelectDialog
                    title={openDialog.title}
                    options={openDialog.options}
                    onSave={val => setField(openDialog.field, val)}
                    onClose={() => setDialog(null)}
                />
            )}
        </div>
    );
}
