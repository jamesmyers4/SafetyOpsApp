import { colors } from '../styles/theme';

interface CalendarPickerProps {
    onSelect: (date: string) => void;
    onClose: () => void;
    /** Offset below the anchoring field. */
    top?: string;
}

/** Day grid for the current month; selecting a day returns it as MM/DD/YYYY. */
export default function CalendarPicker({ onSelect, onClose, top = '40px' }: CalendarPickerProps) {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const days = Array.from({ length: daysInMonth }, (_, i) => i + 1);

    return (
        <div style={{ position: 'absolute', top, left: 0, background: 'white', border: `1px solid ${colors.border}`, borderRadius: '4px', padding: '12px', zIndex: 100, boxShadow: '0 4px 8px rgba(0,0,0,0.15)', minWidth: '240px' }}>
            <div style={{ fontWeight: 'bold', color: colors.navy, marginBottom: '8px', textAlign: 'center' }}>
                {now.toLocaleString('default', { month: 'long' })} {year}
            </div>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px' }}>
                {days.map(d => (
                    <a key={d} href="#"
                        onClick={e => {
                            e.preventDefault();
                            const mm = String(month + 1).padStart(2, '0');
                            const dd = String(d).padStart(2, '0');
                            onSelect(`${mm}/${dd}/${year}`);
                            onClose();
                        }}
                        style={{ display: 'inline-block', width: '30px', textAlign: 'center', padding: '4px', cursor: 'pointer', color: colors.navy, textDecoration: 'none', borderRadius: '3px' }}>
                        {d}
                    </a>
                ))}
            </div>
        </div>
    );
}

/** Invisible full-screen layer that closes an open calendar when clicking outside it. */
export function CalendarBackdrop({ onClose }: { onClose: () => void }) {
    return <div style={{ position: 'fixed', inset: 0, zIndex: 50 }} onClick={onClose} />;
}
