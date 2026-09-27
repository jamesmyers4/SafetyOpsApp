import { useState } from 'react';
import { colors, styles } from '../styles/theme';

interface ConfirmDeleteProps {
    label: string;
    prompt: string;
    onConfirm: () => void;
}

/** A delete button that asks for confirmation inline before calling `onConfirm`. */
export default function ConfirmDelete({ label, prompt, onConfirm }: ConfirmDeleteProps) {
    const [confirming, setConfirming] = useState(false);

    if (!confirming) {
        return <button onClick={() => setConfirming(true)} style={styles.dangerButton}>{label}</button>;
    }
    return (
        <div style={{ display: 'flex', gap: '12px' }}>
            <span style={{ color: colors.danger, fontWeight: 'bold', alignSelf: 'center' }}>{prompt}</span>
            <button onClick={() => { setConfirming(false); onConfirm(); }} style={styles.dangerButton}>Confirm</button>
            <button onClick={() => setConfirming(false)} style={styles.secondaryButton}>Cancel</button>
        </div>
    );
}
