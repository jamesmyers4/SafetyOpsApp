import type { Paged } from '../types/api';
import { colors } from '../styles/theme';

interface PagerProps {
    data: Paged<unknown>;
    noun: [singular: string, plural: string];
    disabled?: boolean;
    onPage: (page: number) => void;
}

const pagerButton = { padding: '6px 14px', borderRadius: '4px', border: `1px solid ${colors.border}`, background: 'white', cursor: 'pointer' };

/** Previous / "Page x of y (n items)" / Next controls for a paged API result. */
export default function Pager({ data, noun, disabled, onPage }: PagerProps) {
    return (
        <div style={{ marginTop: '16px', display: 'flex', gap: '12px', alignItems: 'center', fontSize: '14px', color: colors.muted }}>
            <button onClick={() => onPage(data.page - 1)} disabled={disabled || data.page <= 1} style={pagerButton}>
                Previous
            </button>
            <span>
                Page {data.totalPages === 0 ? 0 : data.page} of {data.totalPages} ({data.totalCount} {data.totalCount === 1 ? noun[0] : noun[1]})
            </span>
            <button onClick={() => onPage(data.page + 1)} disabled={disabled || data.page >= data.totalPages} style={pagerButton}>
                Next
            </button>
        </div>
    );
}
