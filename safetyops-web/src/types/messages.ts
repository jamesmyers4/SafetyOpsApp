// Messages exchanged between shell pages, their iframes, and picker popups via postMessage.

export interface ClassDraft {
    id?: number;
    courseTitle: string;
    courseId: string;
    classDate: string;
    location: string;
}

export interface SelectedStressor {
    stressorId: string;
    stressorName: string;
    examTypeOptions: string[];
}

export type AppMessage =
    | { type: 'trainingReadyToSave'; data: ClassDraft; isUpdate?: boolean }
    | { type: 'trainingFormReset' }
    | { type: 'trainingGoToExisting'; id: number }
    | { type: 'courseSelected'; courseTitle: string; courseId: string }
    | { type: 'personSelected'; id: number; name: string }
    | { type: 'workTasksSelected'; tasks: SelectedStressor[] }
    | { type: 'appointmentUpdated'; message: string }
    | { type: 'appointmentEditCancelled' };
