// Relative URLs: the Vite dev server proxies API calls to ASP.NET Core, and in
// production the API serves the built SPA from the same origin.
async function request<T>(url: string, options?: RequestInit): Promise<T> {
    const response = await fetch(url, {
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

    // Personnel
    addUser: (data: {
        firstName: string; lastName: string; middleName: string; gender: string;
        department: string; employeeCategory: string; subscription: string; employeeNumber: string;
    }) =>
        request<{ success: boolean; message: string; id: number }>('/api/personnel/create', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    getUsers: (search?: string) =>
        request<{ id: number; firstName: string; lastName: string; middleName: string; gender: string; department: string; employeeCategory: string; subscription: string; employeeNumber: string }[]>(
            `/api/personnel/users${search ? `?search=${encodeURIComponent(search)}` : ''}`
        ),

    getUser: (id: number) =>
        request<{ id: number; firstName: string; lastName: string; middleName: string; gender: string; department: string; employeeCategory: string; subscription: string; employeeNumber: string }>(
            `/api/personnel/users/${id}`
        ),

    updateUser: (id: number, data: {
        firstName: string; lastName: string; middleName: string; gender: string;
        department: string; employeeCategory: string; subscription: string; employeeNumber: string;
    }) =>
        request<{ success: boolean; message: string }>(`/api/personnel/users/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    deleteUser: (id: number) =>
        request<{ success: boolean }>(`/api/personnel/users/${id}`, { method: 'DELETE' }),

    // Training
    getTrainingClasses: (search?: string) =>
        request<{ id: number; courseTitle: string; courseId: string; classDate: string; location: string }[]>(
            `/api/training/classes${search ? `?search=${encodeURIComponent(search)}` : ''}`
        ),

    getTrainingClass: (id: number) =>
        request<{ id: number; courseTitle: string; courseId: string; classDate: string; location: string }>(
            `/api/training/classes/${id}`
        ),

    createTrainingClass: (data: { courseTitle: string; courseId: string; classDate: string; location: string }) =>
        request<{ success: boolean; message: string; id: number }>('/api/training/classes', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    updateTrainingClass: (id: number, data: { courseTitle: string; courseId: string; classDate: string; location: string }) =>
        request<{ success: boolean; message: string }>(`/api/training/classes/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    deleteTrainingClass: (id: number) =>
        request<{ success: boolean }>(`/api/training/classes/${id}`, { method: 'DELETE' }),

    getCourses: (search?: string) =>
        request<{ id: string; title: string }[]>(
            `/api/training/courses${search ? `?search=${encodeURIComponent(search)}` : ''}`
        ),

    // Medical surveillance
    getAppointments: (search?: string) =>
        request<{ id: number; date: string; personName: string; personId: number; stressors: { stressorId: string; stressorName: string; examType: string }[] }[]>(
            `/api/medical-surveillance/appointments${search ? `?search=${encodeURIComponent(search)}` : ''}`
        ),

    getAppointment: (id: number) =>
        request<{ id: number; date: string; personName: string; personId: number; stressors: { stressorId: string; stressorName: string; examType: string }[] }>(
            `/api/medical-surveillance/appointments/${id}`
        ),

    createAppointment: (data: { date: string; personName: string; personId: number; stressors: { stressorId: string; stressorName: string; examType: string }[] }) =>
        request<{ success: boolean; message: string; id: number }>('/api/medical-surveillance/appointments', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    updateAppointment: (id: number, data: { date: string; personName: string; personId: number; stressors: { stressorId: string; stressorName: string; examType: string }[] }) =>
        request<{ success: boolean; message: string }>(`/api/medical-surveillance/appointments/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    deleteAppointment: (id: number) =>
        request<{ success: boolean }>(`/api/medical-surveillance/appointments/${id}`, { method: 'DELETE' }),

    getPersonOptions: (search?: string) =>
        request<{ id: number; name: string }[]>(
            `/api/medical-surveillance/persons${search ? `?search=${encodeURIComponent(search)}` : ''}`
        ),

    getWorkTasks: () =>
        request<{ id: string; name: string; stressors: { stressorId: string; stressorName: string }[]; examTypeOptions: string[] }[]>(
            '/api/medical-surveillance/work-tasks'
        ),
};
