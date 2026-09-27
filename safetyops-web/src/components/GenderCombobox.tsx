import { useState } from 'react';
import { colors } from '../styles/theme';
import { GENDERS } from '../pages/personnelOptions';

/** A custom (non-native) combobox: a clickable box that opens a listbox of options. */
export default function GenderCombobox({ value, onChange }: { value: string; onChange: (v: string) => void }) {
    const [open, setOpen] = useState(false);
    return (
        <div style={{ position: 'relative', display: 'inline-block', width: '374px' }}>
            <div
                role="combobox"
                aria-label="Gender"
                aria-expanded={open}
                tabIndex={0}
                onClick={() => setOpen(o => !o)}
                onKeyDown={e => { if (e.key === 'Enter' || e.key === ' ') setOpen(o => !o); }}
                style={{ padding: '10px', border: `1px solid ${colors.border}`, borderRadius: '4px', cursor: 'pointer', background: 'white', fontSize: '14px', userSelect: 'none' }}
            >
                {value || 'Select Gender'}
            </div>
            {open && (
                <ul role="listbox" aria-label="Gender"
                    style={{ position: 'absolute', top: '100%', left: 0, width: '100%', background: 'white', border: `1px solid ${colors.border}`, borderRadius: '4px', zIndex: 100, listStyle: 'none', margin: 0, padding: 0, boxShadow: '0 4px 8px rgba(0,0,0,0.1)' }}>
                    {GENDERS.map(opt => (
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
