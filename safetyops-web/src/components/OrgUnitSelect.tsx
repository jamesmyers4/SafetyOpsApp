import type { CSSProperties } from 'react';
import type { OrgUnit } from '../types/api';

interface OrgUnitSelectProps {
    id: string;
    units: OrgUnit[];
    value: number | undefined;
    onChange: (orgUnitId: number) => void;
    disabled?: boolean;
    style?: CSSProperties;
}

/** Org unit picker, indented to show the tree. Only offer units the user may write to. */
export default function OrgUnitSelect({ id, units, value, onChange, disabled, style }: OrgUnitSelectProps) {
    const byId = new Map(units.map(u => [u.id, u]));
    const depth = (u: OrgUnit): number => (u.parentId !== null && byId.has(u.parentId) ? 1 + depth(byId.get(u.parentId)!) : 0);
    // Parents before children, children in id order.
    const ordered: OrgUnit[] = [];
    const visit = (parentId: number | null) => units
        .filter(u => (u.parentId === parentId) || (parentId === null && u.parentId !== null && !byId.has(u.parentId)))
        .sort((a, b) => a.id - b.id)
        .forEach(u => { ordered.push(u); visit(u.id); });
    visit(null);

    return (
        <select id={id} value={value ?? ''} disabled={disabled} onChange={e => onChange(Number(e.target.value))} style={style}>
            {value !== undefined && !byId.has(value) && <option value={value}>(current unit)</option>}
            {ordered.map(u => (
                <option key={u.id} value={u.id}>{'    '.repeat(depth(u))}{u.name}</option>
            ))}
        </select>
    );
}
