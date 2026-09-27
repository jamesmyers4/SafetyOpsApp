// Relative URLs: the Vite dev server proxies API calls to ASP.NET Core, and in
// production the API serves the built SPA from the same origin.
import type {
    Appointment, AppointmentInput, Course, CurrentUser, Incident, IncidentCategory, IncidentInput, IncidentStatus,
    Paged, Person, PersonInput, PersonOption, TrainingClass, TrainingClassInput, WorkTask,
} from '../types/api';

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
    if (response.status === 401 && !url.startsWith('/api/auth/')) {
        // Session expired or never existed: send the whole app (not just an iframe) to sign in.
        const top = window.top ?? window;
        const returnUrl = encodeURIComponent(top.location.pathname + top.location.search);
        top.location.assign(`/login?returnUrl=${returnUrl}`);
    }
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
// The API sends incident times with seconds; datetime-local inputs use minutes.
const fromApiIncident = (i: Incident): Incident => ({ ...i, occurredAt: i.occurredAt.slice(0, 16) });
const fromApiClass = (c: TrainingClass): TrainingClass => ({ ...c, classDate: fromIsoDate(c.classDate) });
const toApiClass = (c: TrainingClassInput) => ({ ...c, classDate: toIsoDate(c.classDate) });
const fromApiAppointment = (a: Appointment): Appointment => ({ ...a, date: fromIsoDate(a.date) });
const toApiAppointment = (a: AppointmentInput) => ({ ...a, date: toIsoDate(a.date) });

/** Search screens show the first page of matches, up to the API's maximum page size. */
const SEARCH_PAGE_SIZE = 100;

export const api = {
    login: (username: string, password: string) =>
        request<CurrentUser>('/api/auth/login', { method: 'POST', body: json({ username, password }) }),

    me: () =>
        request<CurrentUser>('/api/auth/me'),

    logout: () =>
        request<void>('/api/auth/logout', { method: 'POST' }),

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

    // Incident reports
    listIncidents: async (params: { search?: string; status?: IncidentStatus | ''; category?: IncidentCategory | ''; page?: number; pageSize?: number }) => {
        const page = await request<Paged<Incident>>(`/api/incidents${query(params)}`);
        return { ...page, items: page.items.map(fromApiIncident) };
    },

    getIncident: async (id: number) =>
        fromApiIncident(await request<Incident>(`/api/incidents/${id}`)),

    createIncident: async (data: IncidentInput) =>
        fromApiIncident(await request<Incident>('/api/incidents', { method: 'POST', body: json(data) })),

    updateIncident: async (id: number, data: IncidentInput) =>
        fromApiIncident(await request<Incident>(`/api/incidents/${id}`, { method: 'PUT', body: json(data) })),

    deleteIncident: (id: number) =>
        request<void>(`/api/incidents/${id}`, { method: 'DELETE' }),

    lookupPeople: (search?: string) =>
        request<PersonOption[]>(`/api/personnel/lookup${query({ search })}`),
};
