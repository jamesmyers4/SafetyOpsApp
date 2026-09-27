import { useCallback, useState } from 'react';
import { errorMessage } from '../services/errors';

interface SearchState<T> {
    /** null until the first search completes. */
    results: T[] | null;
    loading: boolean;
    error: string | null;
    run: () => Promise<void>;
}

/** Runs a search on demand and tracks its results, loading flag, and error message. */
export function useSearch<T>(search: () => Promise<T[]>, failureMessage = 'Search failed'): SearchState<T> {
    const [results, setResults] = useState<T[] | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const run = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            setResults(await search());
        } catch (e: unknown) {
            setError(errorMessage(e, failureMessage));
        } finally {
            setLoading(false);
        }
    }, [search, failureMessage]);

    return { results, loading, error, run };
}
