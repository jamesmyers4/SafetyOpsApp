import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { api } from '../services/api';
import type { PersonInput } from '../types/api';
import GenderCombobox from '../components/GenderCombobox';
import LoadStatus from '../components/LoadStatus';
import { errorMessage } from '../services/errors';
import PageLayout from '../components/PageLayout';
import SelectDialog from '../components/SelectDialog';
import { styles } from '../styles/theme';
import { useAccess } from '../auth/currentUser';
import OrgUnitSelect from '../components/OrgUnitSelect';
import { EMPTY_PERSON, PERSON_DIALOGS, type PersonDialog } from './personnelOptions';

const pickerButton = { ...styles.smallButton, marginLeft: '10px' };

export default function EditUserFormPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [dialog, setDialog] = useState<PersonDialog | null>(null);
    const access = useAccess();
    const [form, setForm] = useState<PersonInput>(EMPTY_PERSON);
    const [ownerUnit, setOwnerUnit] = useState<number | undefined>(undefined);
    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState<string | null>(null);
    const [successMsg, setSuccessMsg] = useState('');
    const [errors, setErrors] = useState<string[]>([]);

    useEffect(() => {
        let cancelled = false;
        api.getUser(Number(id))
            .then(({ id: _id, orgUnitName: _unit, ...person }) => { if (!cancelled) { setForm(person); setOwnerUnit(person.orgUnitId); } })
            .catch((e: unknown) => { if (!cancelled) setLoadError(errorMessage(e, 'Failed to load user')); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [id]);

    function setField(field: Exclude<keyof PersonInput, 'orgUnitId'>, value: string) {
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
            setErrors([errorMessage(e, 'Update failed')]);
        }
    }

    const openDialog = dialog && PERSON_DIALOGS[dialog];
    const canEdit = access.canWrite(ownerUnit);

    return (
        <PageLayout section={{ label: 'Personnel', to: '/personnel' }}>
            <h3 style={styles.heading}>Edit User</h3>
            <LoadStatus loading={loading} error={loadError} />
            {!loading && !loadError && !canEdit && (
                <div role="status" style={{ ...styles.alertWarning, marginBottom: '16px' }}>You have read-only access to this record.</div>
            )}

            {errors.length > 0 && (
                <div role="alert" style={styles.errorText}>
                    {errors.map((e, i) => <div key={i}>{e}</div>)}
                </div>
            )}
            {successMsg && (
                <div role="status" style={{ color: 'green', marginBottom: '16px', fontSize: '14px', fontWeight: 'bold' }}>
                    {successMsg}
                </div>
            )}

            <div style={styles.field}>
                <label htmlFor="org-unit" style={styles.label}>Organization Unit</label>
                <OrgUnitSelect id="org-unit" units={canEdit ? access.writableUnits : access.orgUnits} value={form.orgUnitId} disabled={!canEdit}
                    onChange={orgUnitId => setForm(prev => ({ ...prev, orgUnitId }))} style={{ ...styles.input, width: '374px' }} />
            </div>

            <div title="Select a Department" style={styles.field}>
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
                <GenderCombobox value={form.gender} onChange={v => setField('gender', v)} />
            </div>

            <div title="Select an Employee Category" style={styles.field}>
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
            </div>

            <div style={{ display: 'flex', gap: '12px', marginTop: '20px' }}>
                <button onClick={handleUpdate} disabled={loading || !!loadError || !canEdit} style={{ ...styles.primaryButton, padding: '12px 32px' }}>
                    Update
                </button>
                <button onClick={() => navigate('/personnel/edit')} style={{ ...styles.secondaryButton, padding: '12px 32px' }}>
                    Cancel
                </button>
            </div>

            {openDialog && (
                <SelectDialog
                    title={openDialog.title}
                    options={openDialog.options}
                    onSave={val => setField(openDialog.field, val)}
                    onClose={() => setDialog(null)}
                />
            )}
        </PageLayout>
    );
}
