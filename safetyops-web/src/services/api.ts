// Relative URLs: the Vite dev server proxies API calls to ASP.NET Core, and in
// production the API serves the built SPA from the same origin.

export interface Paged<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export interface Person {
    id: number;
    firstName: string;
    lastName: string;
    middleName: string;
    gender: string;
    department: string;
    employeeCategory: string;
    subscription: string;
    employeeNumber: string;
}
export type PersonInput = Omit<Person, 'id'>;

export interface TrainingClass {
    id: number;
    courseTitle: string;
    courseId: string;
    /** MM/DD/YYYY (converted from the API's ISO date) */
    classDate: string;
    location: string;
}
export interface TrainingClassInput {
    courseId: string;
    classDate: string;
    location: string;
}

export interface Course {
    id: string;
    title: string;
}

export interface AppointmentStressor {
    stressorId: string;
    stressorName: string;
    examType: string;
}
export interface Appointment {
    id: number;
    /** MM/DD/YYYY (converted from the API's ISO date) */
    date: string;
    personId: number;
    personName: string;
    stressors: AppointmentStressor[];
}
export interface AppointmentInput {
    date: string;
    personId: number;
    stressors: { stressorId: string; examType: string }[];
}

export interface PersonOption {
    id: number;
    name: string;
}

export interface WorkTask {
    id: string;
    name: string;
    stressors: { stressorId: string; stressorName: string }[];
    examTypeOptions: string[];
}

/** An error response from the API, with the problem-details message flattened for display. */
export class ApiError extends Error {
    readonly status: number;

    constructor(status: number, message: string) {
        super(message);
        this.status = status;
    }
}

interface ProblemDetails {
    title?: string;
    detail?: string;
    errors?: Record<string, string[]>;
}

function problemMessage(problem: ProblemDetails | null): string {
    if (problem?.errors) {
        const messages = Object.values(problem.errors).flat();
        if (messages.length > 0) return messages.join(' ');
    }
    return problem?.detail ?? problem?.title ?? 'Request failed';
}

async function request<T>(url: string, options?: RequestInit): Promise<T> {
    const response = await fetch(url, {
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        ...options,
    });
    if (!response.ok) {
        const problem = await response.json().catch(() => null) as ProblemDetails | null;
        throw new ApiError(response.status, problemMessage(problem));
    }
    if (response.status === 204) return undefined as T;
    return response.json();
}

function query(params: Record<string, string | number | undefined>): string {
    const qs = new URLSearchParams();
    for (const [key, value] of Object.entries(params)) {
        if (value !== undefined && value !== '') qs.set(key, String(value));
    }
    const s = qs.toString();
    return s ? `?${s}` : '';
}

/** 'MM/DD/YYYY' → 'YYYY-MM-DD'. Anything else is passed through for the API to reject. */
export function toIsoDate(date: string): string {
    const m = /^(\d{1,2})\/(\d{1,2})\/(\d{4})$/.exec(date.trim());
    return m ? `${m[3]}-${m[1].padStart(2, '0')}-${m[2].padStart(2, '0')}` : date;
}

/** 'YYYY-MM-DD' → 'MM/DD/YYYY'. */
export function fromIsoDate(date: string): string {
    const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(date);
    return m ? `${m[2]}/${m[3]}/${m[1]}` : date;
}

const json = (body: unknown) => JSON.stringify(body);
const fromApiClass = (c: TrainingClass): TrainingClass => ({ ...c, classDate: fromIsoDate(c.classDate) });
const toApiClass = (c: TrainingClassInput) => ({ ...c, classDate: toIsoDate(c.classDate) });
const fromApiAppointment = (a: Appointment): Appointment => ({ ...a, date: fromIsoDate(a.date) });
const toApiAppointment = (a: AppointmentInput) => ({ ...a, date: toIsoDate(a.date) });

/** Search screens show the first page of matches, up to the API's maximum page size. */
const SEARCH_PAGE_SIZE = 100;

export const api = {
    login: (username: string, password: string) =>
        request<{ success: boolean }>('/auth/login', { method: 'POST', body: json({ username, password }) }),

    checkAuth: () =>
        request<{ authenticated: boolean }>('/auth/check'),

    logout: () =>
        request<{ success: boolean }>('/auth/logout', { method: 'POST' }),

    // Personnel
    listPeople: (params: { search?: string; page?: number; pageSize?: number }) =>
        request<Paged<Person>>(`/api/personnel${query(params)}`),

    getUsers: async (search?: string) =>
        (await request<Paged<Person>>(`/api/personnel${query({ search, pageSize: SEARCH_PAGE_SIZE })}`)).items,

    getUser: (id: number) =>
        request<Person>(`/api/personnel/${id}`),

    addUser: (data: PersonInput) =>
        request<Person>('/api/personnel', { method: 'POST', body: json(data) }),

    updateUser: (id: number, data: PersonInput) =>
        request<Person>(`/api/personnel/${id}`, { method: 'PUT', body: json(data) }),

    deleteUser: (id: number) =>
        request<void>(`/api/personnel/${id}`, { method: 'DELETE' }),

    // Training
    getTrainingClasses: async (search?: string) =>
        (await request<Paged<TrainingClass>>(`/api/training/classes${query({ search, pageSize: SEARCH_PAGE_SIZE })}`)).items.map(fromApiClass),

    getTrainingClass: async (id: number) =>
        fromApiClass(await request<TrainingClass>(`/api/training/classes/${id}`)),

    createTrainingClass: async (data: TrainingClassInput) =>
        fromApiClass(await request<TrainingClass>('/api/training/classes', { method: 'POST', body: json(toApiClass(data)) })),

    updateTrainingClass: async (id: number, data: TrainingClassInput) =>
        fromApiClass(await request<TrainingClass>(`/api/training/classes/${id}`, { method: 'PUT', body: json(toApiClass(data)) })),

    deleteTrainingClass: (id: number) =>
        request<void>(`/api/training/classes/${id}`, { method: 'DELETE' }),

    getCourses: (search?: string) =>
        request<Course[]>(`/api/training/courses${query({ search })}`),

    // Medical surveillance
    getAppointments: async (search?: string) =>
        (await request<Paged<Appointment>>(`/api/medical-surveillance/appointments${query({ search, pageSize: SEARCH_PAGE_SIZE })}`)).items.map(fromApiAppointment),

    getAppointment: async (id: number) =>
        fromApiAppointment(await request<Appointment>(`/api/medical-surveillance/appointments/${id}`)),

    createAppointment: async (data: AppointmentInput) =>
        fromApiAppointment(await request<Appointment>('/api/medical-surveillance/appointments', { method: 'POST', body: json(toApiAppointment(data)) })),

    updateAppointment: async (id: number, data: AppointmentInput) =>
        fromApiAppointment(await request<Appointment>(`/api/medical-surveillance/appointments/${id}`, { method: 'PUT', body: json(toApiAppointment(data)) })),

    deleteAppointment: (id: number) =>
        request<void>(`/api/medical-surveillance/appointments/${id}`, { method: 'DELETE' }),

    getPersonOptions: (search?: string) =>
        request<PersonOption[]>(`/api/medical-surveillance/persons${query({ search })}`),

    getWorkTasks: () =>
        request<WorkTask[]>('/api/medical-surveillance/work-tasks'),
};
