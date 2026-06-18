const BASE_URL = 'https://localhost:7075';

async function request<T>(url: string, options?: RequestInit): Promise<T> {
    const response = await fetch(`${BASE_URL}${url}`, {
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        ...options,
    });
    if (!response.ok) {
        const error = await response.json().catch(() => ({ message: 'Request failed' }));
        throw new Error(error.message ?? 'Request failed');
    }
    return response.json();
}

export const api = {
    login: (username: string, password: string) =>
        request<{ success: boolean }>('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password }),
        }),

    checkAuth: () =>
        request<{ authenticated: boolean }>('/auth/check'),

    logout: () =>
        request<{ success: boolean }>('/auth/logout', { method: 'POST' }),

    addUser: (data: {
        firstName: string;
        lastName: string;
        middleName: string;
        gender: string;
        department: string;
        employeeCategory: string;
        subscription: string;
        employeeNumber: string;
    }) =>
        request<{ success: boolean; message: string }>('/n/safetyops/personnel/create', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    getUsers: () =>
        request<{ firstName: string; lastName: string; department: string; employeeCategory: string }[]>('/n/safetyops/personnel/users'),
};