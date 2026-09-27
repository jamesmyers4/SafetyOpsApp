import type { ReactNode } from 'react';
import NavBar from './NavBar';
import NavLink from './NavLink';
import { styles } from '../styles/theme';

interface PageLayoutProps {
    /** The module link shown in the nav bar next to "Modules". */
    section?: { label: string; to: string };
    /** Shell pages that host an iframe use tighter padding. */
    variant?: 'page' | 'shell';
    children: ReactNode;
}

/** Full-page chrome: background, nav bar with an optional module link, and padded content. */
export default function PageLayout({ section, variant = 'page', children }: PageLayoutProps) {
    return (
        <div style={styles.page}>
            <NavBar extra={section && <NavLink to={section.to}>{section.label}</NavLink>} />
            <div style={variant === 'shell' ? styles.shellContent : styles.content}>
                {children}
            </div>
        </div>
    );
}
