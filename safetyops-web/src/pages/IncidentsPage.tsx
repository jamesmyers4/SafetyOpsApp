import { useEffect, useState } from 'react';
import { api } from '../services/api';
import { errorMessage } from '../services/errors';
import {
    INCIDENT_CATEGORIES, INCIDENT_SEVERITIES, INCIDENT_STATUSES,
    type Incident, type IncidentCategory, type IncidentStatus, type Paged,
} from '../types/api';
import LoadStatus from '../components/LoadStatus';
import NavLink from '../components/NavLink';
import PageLayout from '../components/PageLayout';
import Pager from '../components/Pager';
import { styles } from '../styles/theme';
import { formatIncidentTime } from './incidentFormat';

const PAGE_SIZE = 25;
const filterSelect = { ...styles.input, width: '180px' };

export default function IncidentsPage() {
    const [data, setData] = useState<Paged<Incident> | null>(null);
    const [search, setSearch] = useState('');
    const [filters, setFilters] = useState<{ search: string; status: IncidentStatus | ''; category: IncidentCategory | ''; page: number }>(
        { search: '', status: '', category: '', page: 1 });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        api.listIncidents({ ...filters, pageSize: PAGE_SIZE })
            .then(result => { if (!cancelled) { setData(result); setError(null); } })
            .catch((e: unknown) => { if (!cancelled) setError(errorMessage(e, 'Failed to load incidents')); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [filters]);

    function update(changes: Partial<typeof filters>) {
        setLoading(true);
        setFilters(prev => ({ ...prev, page: 1, ...changes }));
    }

    return (
        <PageLayout section={{ label: 'Incident Reports', to: '/incidents' }}>
            <h2 style={{ ...styles.heading, marginBottom: '20px' }}>Incident Reports</h2>
            <div style={styles.field}>
                <NavLink to="/incidents/new" role="link" style={styles.buttonLink}>Create Incident</NavLink>
            </div>

            <div style={{ ...styles.buttonRow, marginBottom: '20px' }}>
                <input type="text" placeholder="Search location, description, or reporter..." aria-label="Search incidents"
                    value={search} onChange={e => setSearch(e.target.value)}
                    onKeyDown={e => { if (e.key === 'Enter') update({ search: search.trim() }); }}
                    style={{ ...styles.input, width: '320px' }} />
                <button onClick={() => update({ search: search.trim() })} style={{ ...styles.smallButton, padding: '10px 20px' }}>Search</button>
                <select aria-label="Filter by status" value={filters.status} onChange={e => update({ status: e.target.value as IncidentStatus | '' })} style={filterSelect}>
                    <option value="">All statuses</option>
                    {Object.entries(INCIDENT_STATUSES).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                </select>
                <select aria-label="Filter by category" value={filters.category} onChange={e => update({ category: e.target.value as IncidentCategory | '' })} style={filterSelect}>
                    <option value="">All categories</option>
                    {Object.entries(INCIDENT_CATEGORIES).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                </select>
            </div>

            <LoadStatus loading={loading && !data} error={error} />
            {data && data.items.length === 0 && <p style={styles.emptyText}>No incidents match.</p>}

            {data && data.items.length > 0 && (
                <>
                    <table style={styles.cardTable}>
                        <thead>
                            <tr>
                                <th style={styles.th}>#</th>
                                <th style={styles.th}>Occurred</th>
                                <th style={styles.th}>Category</th>
                                <th style={styles.th}>Severity</th>
                                <th style={styles.th}>Location</th>
                                <th style={styles.th}>Reported By</th>
                                <th style={styles.th}>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {data.items.map(i => (
                                <tr key={i.id}>
                                    <td style={styles.td}><NavLink to={`/incidents/${i.id}`} style={styles.boldLink}>{i.id}</NavLink></td>
                                    <td style={styles.td}>{formatIncidentTime(i.occurredAt)}</td>
                                    <td style={styles.td}>{INCIDENT_CATEGORIES[i.category]}</td>
                                    <td style={styles.td}>{INCIDENT_SEVERITIES[i.severity]}</td>
                                    <td style={styles.td}>{i.location}</td>
                                    <td style={styles.td}>{i.reportedByName}</td>
                                    <td style={styles.td}>{INCIDENT_STATUSES[i.status]}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <Pager data={data} noun={['incident', 'incidents']} disabled={loading}
                        onPage={page => { setLoading(true); setFilters(prev => ({ ...prev, page })); }} />
                </>
            )}
        </PageLayout>
    );
}
