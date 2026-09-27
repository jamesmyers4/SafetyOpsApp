import { useState } from 'react';
import { colors, styles } from '../styles/theme';

interface SelectDialogProps {
    title: string;
    options: string[];
    onSave: (value: string) => void;
    onClose: () => void;
}

/** Modal list with single-select checkboxes; Save applies the choice, the ✕ or backdrop cancels. */
export default function SelectDialog({ title, options, onSave, onClose }: SelectDialogProps) {
    const [selected, setSelected] = useState('');
    return (
        <div onClick={e => { if (e.target === e.currentTarget) onClose(); }}
            style={{ position: 'fixed', top: 0, left: 0, width: '100%', height: '100%', background: 'rgba(0,0,0,0.5)', display: 'flex', justifyContent: 'center', alignItems: 'center', zIndex: 999 }}>
            <div role="dialog" aria-label={title} style={{ position: 'relative', background: 'white', borderRadius: '8px', padding: '30px', width: '500px', maxHeight: '400px', overflowY: 'auto' }}>
                <h4 style={{ marginTop: 0, color: colors.navy }}>{title}</h4>
                <button onClick={onClose}
                    style={{ position: 'absolute', top: '12px', right: '16px', background: 'none', border: 'none', fontSize: '20px', color: 'red', cursor: 'pointer', fontWeight: 'bold', zIndex: 1000 }}>
                    &#10005;
                </button>
                <table style={styles.table}>
                    <thead>
                        <tr>
                            <th style={styles.compactTh}>Select</th>
                            <th style={styles.compactTh}>Option</th>
                        </tr>
                    </thead>
                    <tbody>
                        {options.map(opt => (
                            <tr key={opt}>
                                <td style={styles.compactTd}>
                                    <input type="checkbox" checked={selected === opt} onChange={() => setSelected(selected === opt ? '' : opt)} />
                                </td>
                                <td style={styles.compactTd}>{opt}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                <button onClick={() => { onSave(selected); onClose(); }} style={{ ...styles.smallButton, padding: '10px 24px', marginTop: '16px' }}>
                    Save
                </button>
            </div>
        </div>
    );
}
