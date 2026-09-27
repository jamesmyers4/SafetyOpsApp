import { useEffect, useState, type FormEvent } from 'react';
import { api } from '../services/api';
import {
    INCIDENT_CATEGORIES, INCIDENT_SEVERITIES, INCIDENT_STATUSES,
    type IncidentInput, type PersonOption,
} from '../types/api';
import { styles } from '../styles/theme';
import { useAccess } from '../auth/currentUser';
import OrgUnitSelect from './OrgUnitSelect';

interface IncidentFormProps {
    initial: IncidentInput;
    /** Status is only editable on an existing incident. */
    showStatus: boolean;
    submitLabel: string;
    errors: string[];
    onSubmit: (input: IncidentInput) => void;
    onCancel?: () => void;
}

const select = { ...styles.input, width: '374px' };

/** Fields for reporting or editing an incident, with client-side required checks. */
export default function IncidentForm({ initial, showStatus, submitLabel, errors, onSubmit, onCancel }: IncidentFormProps) {
    const [form, setForm] = useState(initial);
    const [people, setPeople] = useState<PersonOption[]>([]);
    const [localErrors, setLocalErrors] = useState<string[]>([]);
    const access = useAccess();

    useEffect(() => {
        api.lookupPeople().then(setPeople).catch(() => setPeople([]));
    }, []);

    function set<K extends keyof IncidentInput>(field: K, value: IncidentInput[K]) {
        setForm(prev => ({ ...prev, [field]: value }));
    }

    function handleSubmit(e: FormEvent) {
        e.preventDefault();
        const errs: string[] = [];
        if (!form.occurredAt) errs.push('Date and time are required.');
        if (!form.location.trim()) errs.push('Location is required.');
        if (!form.description.trim()) errs.push('Description is required.');
        if (!form.reportedById) errs.push('Reported by is required.');
        if (!form.orgUnitId) errs.push('Organization unit is required.');
        setLocalErrors(errs);
        if (errs.length === 0) onSubmit(form);
    }

    const shown = localErrors.length > 0 ? localErrors : errors;

    return (
        <form onSubmit={handleSubmit}>
            {shown.length > 0 && (
                <div role="alert" style={styles.errorText}>
                    {shown.map((e, i) => <div key={i}>{e}</div>)}
                </div>
            )}

            <div style={styles.field}>
                <label htmlFor="incident-occurred-at" style={styles.label}>Date and Time</label>
                <input id="incident-occurred-at" type="datetime-local" value={form.occurredAt}
                    onChange={e => set('occurredAt', e.target.value)} style={styles.input} />
            </div>

            <div style={styles.field}>
                <label htmlFor="incident-org-unit" style={styles.label}>Organization Unit</label>
                <OrgUnitSelect id="incident-org-unit" units={access.writableUnits} value={form.orgUnitId || undefined}
                    onChange={orgUnitId => set('orgUnitId', orgUnitId)} style={select} />
            </div>

            <div style={styles.field}>
                <label htmlFor="incident-location" style={styles.label}>Location</label>
                <input id="incident-location" type="text" value={form.location} maxLength={200}
                    onChange={e => set('location', e.target.value)} style={styles.input} />
            </div>

            <div style={styles.field}>
                <label htmlFor="incident-category" style={styles.label}>Category</label>
                <select id="incident-category" value={form.category} onChange={e => set('category', e.target.value as IncidentInput['category'])} style={select}>
                    {Object.entries(INCIDENT_CATEGORIES).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                </select>
            </div>

            <div style={styles.field}>
                <label htmlFor="incident-severity" style={styles.label}>Severity</label>
                <select id="incident-severity" value={form.severity} onChange={e => set('severity', e.target.value as IncidentInput['severity'])} style={select}>
                    {Object.entries(INCIDENT_SEVERITIES).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                </select>
            </div>

            <div style={styles.field}>
                <label htmlFor="incident-reported-by" style={styles.label}>Reported By</label>
                <select id="incident-reported-by" value={form.reportedById || ''} onChange={e => set('reportedById', Number(e.target.value))} style={select}>
                    <option value="">Select a person</option>
                    {people.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                </select>
            </div>

            {showStatus && (
                <div style={styles.field}>
                    <label htmlFor="incident-status" style={styles.label}>Status</label>
                    <select id="incident-status" value={form.status} onChange={e => set('status', e.target.value as IncidentInput['status'])} style={select}>
                        {Object.entries(INCIDENT_STATUSES).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                    </select>
                </div>
            )}

            <div style={styles.field}>
                <label htmlFor="incident-description" style={styles.label}>Description</label>
                <textarea id="incident-description" value={form.description} maxLength={2000} rows={5}
                    onChange={e => set('description', e.target.value)} style={{ ...styles.input, width: '500px', fontFamily: 'inherit' }} />
            </div>

            <div style={{ display: 'flex', gap: '12px' }}>
                <button type="submit" style={styles.primaryButton}>{submitLabel}</button>
                {onCancel && <button type="button" onClick={onCancel} style={styles.secondaryButton}>Cancel</button>}
            </div>
        </form>
    );
}
