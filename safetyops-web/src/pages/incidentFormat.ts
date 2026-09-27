/** `YYYY-MM-DDTHH:mm` → `MM/DD/YYYY HH:mm`, matching how dates read elsewhere in the app. */
export function formatIncidentTime(value: string): string {
    const m = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/.exec(value);
    return m ? `${m[2]}/${m[3]}/${m[1]} ${m[4]}:${m[5]}` : value;
}

/** Current local time as a datetime-local value. */
export function nowForInput(): string {
    const d = new Date();
    d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
    return d.toISOString().slice(0, 16);
}
