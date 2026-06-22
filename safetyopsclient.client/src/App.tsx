import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import SplashPage from './pages/SplashPage';
import LoginPage from './pages/LoginPage';
import MainPage from './pages/MainPage';
import PersonnelHomePage from './pages/PersonnelHomePage';
import AddUserPage from './pages/AddUserPage';
import SuccessPage from './pages/SuccessPage';
import EditUserSearchPage from './pages/EditUserSearchPage';
import EditUserFormPage from './pages/EditUserFormPage';
import AccessLevelsPage from './pages/AccessLevelsPage';
import TrainingAdminShellPage from './pages/TrainingAdminShellPage';
import CreateClassFrame from './pages/CreateClassFrame';
import EditClassFrame from './pages/EditClassFrame';
import CoursePickerPage from './pages/CoursePickerPage';
import ClassDetailPage from './pages/ClassDetailPage';
import OMSSShellPage from './pages/OMSSShellPage';
import OMSSCreatePage from './pages/OMSSCreatePage';
import OMSSEditPage from './pages/OMSSEditPage';
import CreateOMSSFrame from './pages/CreateOMSSFrame';
import EditOMSSFrame from './pages/EditOMSSFrame';
import PersonPickerPage from './pages/PersonPickerPage';
import WorkTaskPickerPage from './pages/WorkTaskPickerPage';
import AppointmentPage from './pages/AppointmentPage';
import FirPage from './pages/FirPage';
import FirCreatePage from './pages/FirCreatePage';
import FirIncidentPage from './pages/FirIncidentPage';

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Core */}
                <Route path="/" element={<SplashPage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/n/safetyops/main" element={<MainPage />} />

                {/* Personnel */}
                <Route path="/n/safetyops/personnel" element={<PersonnelHomePage />} />
                <Route path="/n/safetyops/personnel/create" element={<AddUserPage />} />
                <Route path="/n/safetyops/personnel/success" element={<SuccessPage />} />
                <Route path="/n/safetyops/personnel/edit" element={<EditUserSearchPage />} />
                <Route path="/n/safetyops/personnel/edit/:id" element={<EditUserFormPage />} />
                <Route path="/n/safetyops/personnel/access" element={<AccessLevelsPage />} />

                {/* Training Admin — shell and frames */}
                <Route path="/n/safetyops/training" element={<TrainingAdminShellPage />} />
                <Route path="/n/safetyops/training/create-frame" element={<CreateClassFrame />} />
                <Route path="/n/safetyops/training/edit-frame" element={<EditClassFrame />} />
                <Route path="/n/safetyops/training/course-picker" element={<CoursePickerPage />} />
                <Route path="/n/safetyops/training/class/:id" element={<ClassDetailPage />} />

                {/* OMSS — shell, sub-pages, and frames */}
                <Route path="/n/safetyops/omss" element={<OMSSShellPage />} />
                <Route path="/n/safetyops/omss/create" element={<OMSSCreatePage />} />
                <Route path="/n/safetyops/omss/edit" element={<OMSSEditPage />} />
                <Route path="/n/safetyops/omss/create-frame" element={<CreateOMSSFrame />} />
                <Route path="/n/safetyops/omss/edit-frame" element={<EditOMSSFrame />} />
                <Route path="/n/safetyops/omss/person-picker" element={<PersonPickerPage />} />
                <Route path="/n/safetyops/omss/work-task-picker" element={<WorkTaskPickerPage />} />
                <Route path="/n/safetyops/omss/appointment/:id" element={<AppointmentPage />} />

                {/* FIR */}
                <Route path="/n/safetyops/fir" element={<FirPage />} />
                <Route path="/n/safetyops/fir/create" element={<FirCreatePage />} />
                <Route path="/n/safetyops/fir/incident/:id" element={<FirIncidentPage />} />

                <Route path="*" element={<Navigate to="/" />} />
            </Routes>
        </BrowserRouter>
    );
}
