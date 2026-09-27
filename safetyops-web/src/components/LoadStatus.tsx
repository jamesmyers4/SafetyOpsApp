import { styles } from '../styles/theme';

interface LoadStatusProps {
    loading?: boolean;
    error?: string | null;
}

/** Shows "Loading…" while a request is in flight and the error message if it failed. */
export default function LoadStatus({ loading, error }: LoadStatusProps) {
    if (error) return <div role="alert" style={styles.errorText}>{error}</div>;
    if (loading) return <p role="status" style={styles.emptyText}>Loading…</p>;
    return null;
}
