import type { CSSProperties, ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { styles } from '../styles/theme';

interface NavLinkProps {
    to: string;
    children: ReactNode;
    role?: string;
    style?: CSSProperties;
}

/** An in-app link rendered as an anchor (the E2E suite locates these as links). Defaults to the nav bar style. */
export default function NavLink({ to, children, role, style = styles.navLink }: NavLinkProps) {
    const navigate = useNavigate();
    return (
        <a href={to} role={role} style={style} onClick={e => { e.preventDefault(); navigate(to); }}>
            {children}
        </a>
    );
}
