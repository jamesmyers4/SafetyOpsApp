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
