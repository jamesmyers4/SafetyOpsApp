import type { CSSProperties } from 'react';

export const colors = {
    navy: '#1a2744',
    navLink: '#aac4ff',
    pageBg: '#f4f6f9',
    text: '#333',
    muted: '#555',
    subtle: '#666',
    border: '#ccc',
    rowBorder: '#eee',
    danger: '#cc0000',
    readOnlyBg: '#f5f5f5',
};

const button: CSSProperties = { color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' };

export const styles = {
    page: { background: colors.pageBg, minHeight: '100vh', margin: 0 },
    /** Content padding for full pages and for shell pages that host an iframe. */
    content: { padding: '40px 60px' },
    shellContent: { padding: '20px 30px' },
    /** Body of pages that render inside an iframe or popup. */
    frame: { fontFamily: 'Arial, sans-serif', padding: '20px', background: 'white', minHeight: '100vh' },
    heading: { color: colors.navy, marginBottom: '30px' },
    frameHeading: { color: colors.navy, marginTop: 0 },

    navLink: { color: colors.navLink, textDecoration: 'none', fontSize: '15px', fontWeight: 'normal', cursor: 'pointer' },
    textLink: { color: colors.navy, textDecoration: 'underline', cursor: 'pointer' },
    boldLink: { color: colors.navy, fontWeight: 'bold', textDecoration: 'underline', cursor: 'pointer' },
    /** A link styled as a solid button (module landing pages). */
    buttonLink: { background: colors.navy, color: 'white', textDecoration: 'none', padding: '10px 24px', borderRadius: '4px', fontSize: '15px' },

    primaryButton: { ...button, background: colors.navy, padding: '10px 28px', fontSize: '15px' },
    secondaryButton: { ...button, background: colors.muted, padding: '10px 20px', fontSize: '15px' },
    dangerButton: { ...button, background: colors.danger, padding: '10px 24px', fontSize: '15px' },
    smallButton: { ...button, background: colors.navy, padding: '8px 16px' },
    smallDangerButton: { ...button, background: colors.danger, padding: '6px 14px' },

    label: { display: 'block', marginBottom: '6px', fontWeight: 'bold', color: colors.text },
    frameLabel: { display: 'block', marginBottom: '4px', fontWeight: 'bold' },
    input: { width: '350px', padding: '10px', border: `1px solid ${colors.border}`, borderRadius: '4px', fontSize: '14px' },
    frameInput: { padding: '8px', width: '350px', border: `1px solid ${colors.border}`, borderRadius: '4px', fontSize: '14px' },
    searchInput: { padding: '10px', width: '360px', border: `1px solid ${colors.border}`, borderRadius: '4px', fontSize: '14px' },
    field: { marginBottom: '20px' },
    frameField: { marginBottom: '16px' },

    table: { width: '100%', borderCollapse: 'collapse' },
    cardTable: { width: '100%', borderCollapse: 'collapse', background: 'white', borderRadius: '8px', overflow: 'hidden', boxShadow: '0 2px 8px rgba(0,0,0,0.1)' },
    th: { background: colors.navy, color: 'white', padding: '12px 16px', textAlign: 'left' },
    td: { padding: '12px 16px', borderBottom: `1px solid ${colors.rowBorder}` },
    compactTh: { background: colors.navy, color: 'white', padding: '10px', textAlign: 'left' },
    compactTd: { padding: '10px', borderBottom: `1px solid ${colors.rowBorder}` },

    card: { background: 'white', padding: '24px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)', marginBottom: '24px' },
    errorText: { color: 'red', marginBottom: '16px', fontSize: '14px' },
    emptyText: { color: colors.subtle },
    alertSuccess: { background: '#d4edda', color: '#155724', padding: '12px 20px', borderRadius: '4px', marginBottom: '16px', fontSize: '14px' },
    alertDanger: { background: '#f8d7da', color: '#721c24', padding: '12px 20px', borderRadius: '4px', marginBottom: '16px', fontSize: '14px' },
    alertWarning: { padding: '16px', background: '#fff3cd', border: '1px solid #ffc107', borderRadius: '4px' },
    buttonRow: { display: 'flex', gap: '12px', alignItems: 'center', flexWrap: 'wrap' },
} satisfies Record<string, CSSProperties>;
