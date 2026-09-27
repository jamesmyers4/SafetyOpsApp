/** Turns anything thrown by the API client into a display message. */
export function errorMessage(e: unknown, fallback: string): string {
    return e instanceof Error ? e.message : fallback;
}
