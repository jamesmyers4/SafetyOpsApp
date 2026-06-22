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

    // Personnel
    addUser: (data: {
        firstName: string; lastName: string; middleName: string; gender: string;
        department: string; employeeCategory: string; subscription: string; employeeNumber: string;
    }) =>
        request<{ success: boolean; message: string; id: number }>('/n/safetyops/personnel/create', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    getUsers: (search?: string) =>
        request<{ id: number; firstName: string; lastName: string; middleName: string; gender: string; department: string; employeeCategory: string; subscription: string; employeeNumber: string }[]>(
            `/n/safetyops/personnel/users${search ? `?search=${encodeURIComponent(search)}` : ''}`
        ),

    getUser: (id: number) =>
        request<{ id: number; firstName: string; lastName: string; middleName: string; gender: string; department: string; employeeCategory: string; subscription: string; employeeNumber: string }>(
            `/n/safetyops/personnel/users/${id}`
        ),

    updateUser: (id: number, data: {
        firstName: string; lastName: string; middleName: string; gender: string;
        department: string; employeeCategory: string; subscription: string; employeeNumber: string;
    }) =>
        request<{ success: boolean; message: string }>(`/n/safetyops/personnel/users/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    deleteUser: (id: number) =>
        request<{ success: boolean }>(`/n/safetyops/personnel/users/${id}`, { method: 'DELETE' }),

    // Training
    getTrainingClasses: (search?: string) =>
        request<{ id: number; courseTitle: string; courseId: string; classDate: string; location: string }[]>(
            `/n/safetyops/training/classes${search ? `?search=${encodeURIComponent(search)}` : ''}`
        ),

    getTrainingClass: (id: number) =>
        request<{ id: number; courseTitle: string; courseId: string; classDate: string; location: string }>(
            `/n/safetyops/training/classes/${id}`
        ),

    createTrainingClass: (data: { courseTitle: string; courseId: string; classDate: string; location: string }) =>
        request<{ success: boolean; message: string; id: number }>('/n/safetyops/training/classes', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    updateTrainingClass: (id: number, data: { courseTitle: string; courseId: string; classDate: string; location: string }) =>
        request<{ success: boolean; message: string }>(`/n/safetyops/training/classes/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    deleteTrainingClass: (id: number) =>
        request<{ success: boolean }>(`/n/safetyops/training/classes/${id}`, { method: 'DELETE' }),

    getCourses: (search?: string) =>
        request<{ id: string; title: string }[]>(
            `/n/safetyops/training/courses${search ? `?search=${encodeURIComponent(search)}` : ''}`
        ),

    // OMSS
    getOmssAppointments: (search?: string) =>
        request<{ id: number; date: string; personName: string; personId: number; stressors: { stressorId: string; stressorName: string; examType: string }[] }[]>(
            `/n/safetyops/omss/appointments${search ? `?search=${encodeURIComponent(search)}` : ''}`
        ),

    getOmssAppointment: (id: number) =>
        request<{ id: number; date: string; personName: string; personId: number; stressors: { stressorId: string; stressorName: string; examType: string }[] }>(
            `/n/safetyops/omss/appointments/${id}`
        ),

    createOmssAppointment: (data: { date: string; personName: string; personId: number; stressors: { stressorId: string; stressorName: string; examType: string }[] }) =>
        request<{ success: boolean; message: string; id: number }>('/n/safetyops/omss/appointments', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    updateOmssAppointment: (id: number, data: { date: string; personName: string; personId: number; stressors: { stressorId: string; stressorName: string; examType: string }[] }) =>
        request<{ success: boolean; message: string }>(`/n/safetyops/omss/appointments/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    deleteOmssAppointment: (id: number) =>
        request<{ success: boolean }>(`/n/safetyops/omss/appointments/${id}`, { method: 'DELETE' }),

    getOmssPersons: (search?: string) =>
        request<{ id: number; name: string }[]>(
            `/n/safetyops/omss/persons${search ? `?search=${encodeURIComponent(search)}` : ''}`
        ),

    getOmssWorkTasks: () =>
        request<{ id: string; name: string; stressors: { stressorId: string; stressorName: string }[]; examTypeOptions: string[] }[]>(
            '/n/safetyops/omss/worktasks'
        ),
};
