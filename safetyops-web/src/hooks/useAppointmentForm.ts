import { useCallback, useState } from 'react';
import { useAppMessages } from '../services/messaging';
import type { Appointment, AppointmentInput } from '../types/api';

export const ALL_EXAM_TYPES = ['Initial', 'Periodic', 'Exit', 'Return to Duty', 'Special'];

export interface StressorRow {
    stressorId: string;
    stressorName: string;
    examType: string;
    examTypeOptions: string[];
}

/**
 * State for the appointment create/edit frames. The person comes from a picker popup and
 * stressors from the work-task picker iframe; both report back through postMessage.
 */
export function useAppointmentForm() {
    const [date, setDate] = useState('');
    const [personName, setPersonName] = useState('');
    const [personId, setPersonId] = useState(0);
    const [stressors, setStressors] = useState<StressorRow[]>([]);
    const [showWorkTaskPicker, setShowWorkTaskPicker] = useState(false);

    useAppMessages(message => {
        if (message.type === 'personSelected') {
            setPersonName(message.name);
            setPersonId(message.id);
        } else if (message.type === 'workTasksSelected') {
            setStressors(prev => [
                ...prev,
                ...message.tasks
                    .filter(t => !prev.some(p => p.stressorId === t.stressorId))
                    .map(t => ({ ...t, examType: '' })),
            ]);
            setShowWorkTaskPicker(false);
        }
    });

    const load = useCallback((appointment: Appointment) => {
        setDate(appointment.date);
        setPersonName(appointment.personName);
        setPersonId(appointment.personId);
        setStressors(appointment.stressors.map(s => ({ ...s, examTypeOptions: ALL_EXAM_TYPES })));
    }, []);

    function setExamType(stressorId: string, examType: string) {
        setStressors(prev => prev.map(s => (s.stressorId === stressorId ? { ...s, examType } : s)));
    }

    function validate(): string[] {
        const errors: string[] = [];
        if (!date) errors.push('Appointment Date is required.');
        if (!personId) errors.push('Person Evaluated is required.');
        return errors;
    }

    function toInput(): AppointmentInput {
        return { date, personId, stressors: stressors.map(s => ({ stressorId: s.stressorId, examType: s.examType })) };
    }

    return {
        date, setDate, personName, stressors, setExamType,
        showWorkTaskPicker, openWorkTaskPicker: () => setShowWorkTaskPicker(true),
        load, validate, toInput,
    };
}

export type AppointmentForm = ReturnType<typeof useAppointmentForm>;
