/** Client-side checks for a class date typed as MM/DD/YYYY (the API enforces the same rules). */
export function validateClassDate(date: string): string | null {
    if (!date) return 'Class Date is required.';
    const parsed = new Date(date);
    if (isNaN(parsed.getTime())) return 'Invalid date. Please enter a valid date.';
    if (parsed > new Date()) return 'Future dates are not allowed.';
    return null;
}

export function validateClassForm(courseId: string, classDate: string, location: string): string[] {
    const errors: string[] = [];
    if (!courseId.trim()) errors.push('Course ID is required.');
    const dateError = validateClassDate(classDate);
    if (dateError) errors.push(dateError);
    if (!location.trim()) errors.push('Specific location is required');
    return errors;
}
