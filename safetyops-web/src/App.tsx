import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import RequireAuth from './auth/RequireAuth';
import SplashPage from './pages/SplashPage';
import LoginPage from './pages/LoginPage';
import MainPage from './pages/MainPage';
import PersonnelHomePage from './pages/PersonnelHomePage';
import AddUserPage from './pages/AddUserPage';
import SuccessPage from './pages/SuccessPage';
import EditUserSearchPage from './pages/EditUserSearchPage';
import EditUserFormPage from './pages/EditUserFormPage';
import AccessLevelsPage from './pages/AccessLevelsPage';
import TrainingShellPage from './pages/TrainingShellPage';
import CreateClassFrame from './pages/CreateClassFrame';
import EditClassFrame from './pages/EditClassFrame';
import CoursePickerPage from './pages/CoursePickerPage';
import ClassDetailPage from './pages/ClassDetailPage';
import MedicalSurveillancePage from './pages/MedicalSurveillancePage';
import MedicalCreatePage from './pages/MedicalCreatePage';
import MedicalEditPage from './pages/MedicalEditPage';
import CreateAppointmentFrame from './pages/CreateAppointmentFrame';
import EditAppointmentFrame from './pages/EditAppointmentFrame';
import PersonPickerPage from './pages/PersonPickerPage';
import WorkTaskPickerPage from './pages/WorkTaskPickerPage';
import AppointmentPage from './pages/AppointmentPage';
import IncidentsPage from './pages/IncidentsPage';
import IncidentCreatePage from './pages/IncidentCreatePage';
import IncidentDetailPage from './pages/IncidentDetailPage';

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Core */}
                <Route path="/" element={<SplashPage />} />
                <Route path="/login" element={<LoginPage />} />

                {/* Everything below requires a signed-in user */}
                <Route element={<RequireAuth />}>
                    <Route path="/home" element={<MainPage />} />

                    {/* Personnel */}
                    <Route path="/personnel" element={<PersonnelHomePage />} />
                    <Route path="/personnel/create" element={<AddUserPage />} />
                    <Route path="/personnel/success" element={<SuccessPage />} />
                    <Route path="/personnel/edit" element={<EditUserSearchPage />} />
                    <Route path="/personnel/edit/:id" element={<EditUserFormPage />} />
                    <Route path="/personnel/access-levels" element={<AccessLevelsPage />} />

                    {/* Training — shell and frames */}
                    <Route path="/training" element={<TrainingShellPage />} />
                    <Route path="/training/create-frame" element={<CreateClassFrame />} />
                    <Route path="/training/edit-frame" element={<EditClassFrame />} />
                    <Route path="/training/course-picker" element={<CoursePickerPage />} />
                    <Route path="/training/classes/:id" element={<ClassDetailPage />} />

                    {/* Medical surveillance — shell, sub-pages, and frames */}
                    <Route path="/medical-surveillance" element={<MedicalSurveillancePage />} />
                    <Route path="/medical-surveillance/create" element={<MedicalCreatePage />} />
                    <Route path="/medical-surveillance/edit" element={<MedicalEditPage />} />
                    <Route path="/medical-surveillance/create-frame" element={<CreateAppointmentFrame />} />
                    <Route path="/medical-surveillance/edit-frame" element={<EditAppointmentFrame />} />
                    <Route path="/medical-surveillance/person-picker" element={<PersonPickerPage />} />
                    <Route path="/medical-surveillance/work-task-picker" element={<WorkTaskPickerPage />} />
                    <Route path="/medical-surveillance/appointments/:id" element={<AppointmentPage />} />

                    {/* Incident reports */}
                    <Route path="/incidents" element={<IncidentsPage />} />
                    <Route path="/incidents/new" element={<IncidentCreatePage />} />
                    <Route path="/incidents/:id" element={<IncidentDetailPage />} />
                </Route>

                <Route path="*" element={<Navigate to="/" />} />
            </Routes>
        </BrowserRouter>
    );
}
