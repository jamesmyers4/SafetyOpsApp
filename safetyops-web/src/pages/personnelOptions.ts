// Choices offered by the Add User and Edit User forms.
import type { PersonInput } from '../types/api';

export const DEPARTMENTS = ['Engineering', 'Operations', 'Human Resources', 'Finance', 'Safety'];
export const SUBSCRIPTIONS = ['Basic', 'Standard', 'Premium'];
export const EMPLOYEE_CATEGORIES = ['Full Time', 'Part Time', 'Contractor', 'Intern'];
export const GENDERS = ['Male', 'Female', 'Non-binary', 'Prefer not to say'];

export type PersonDialog = 'department' | 'subscription' | 'category';

export const PERSON_DIALOGS: Record<PersonDialog, { title: string; options: string[]; field: 'department' | 'subscription' | 'employeeCategory' }> = {
    department: { title: 'Select a Department', options: DEPARTMENTS, field: 'department' },
    subscription: { title: 'Subscriptions', options: SUBSCRIPTIONS, field: 'subscription' },
    category: { title: 'Select an Employee Category', options: EMPLOYEE_CATEGORIES, field: 'employeeCategory' },
};

export const EMPTY_PERSON: PersonInput = {
    firstName: '', lastName: '', middleName: '', gender: '',
    department: '', employeeCategory: '', subscription: '', employeeNumber: '',
};
