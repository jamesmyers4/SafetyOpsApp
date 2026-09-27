// Request and response shapes for the SafetyOps API. Dates in these types are the UI's
// MM/DD/YYYY strings; services/api.ts converts to and from the API's ISO dates.

export interface CurrentUser {
    id: number;
    userName: string;
    displayName: string;
}

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

export const INCIDENT_CATEGORIES = {
    Fire: 'Fire',
    Injury: 'Injury',
    NearMiss: 'Near Miss',
    PropertyDamage: 'Property Damage',
    Environmental: 'Environmental',
    Other: 'Other',
} as const;
export type IncidentCategory = keyof typeof INCIDENT_CATEGORIES;

export const INCIDENT_SEVERITIES = { Low: 'Low', Medium: 'Medium', High: 'High', Critical: 'Critical' } as const;
export type IncidentSeverity = keyof typeof INCIDENT_SEVERITIES;

export const INCIDENT_STATUSES = { Open: 'Open', UnderReview: 'Under Review', Closed: 'Closed' } as const;
export type IncidentStatus = keyof typeof INCIDENT_STATUSES;

export interface Incident {
    id: number;
    /** Local time at the site, `YYYY-MM-DDTHH:mm` (the format of a datetime-local input). */
    occurredAt: string;
    location: string;
    category: IncidentCategory;
    severity: IncidentSeverity;
    description: string;
    reportedById: number;
    reportedByName: string;
    status: IncidentStatus;
}
export type IncidentInput = Omit<Incident, 'id' | 'reportedByName'>;
