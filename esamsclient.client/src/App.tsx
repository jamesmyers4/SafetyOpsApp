import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import SplashPage from './pages/SplashPage';
import LoginPage from './pages/LoginPage';
import MainPage from './pages/MainPage';
import PersonnelHomePage from './pages/PersonnelHomePage';
import AddUserPage from './pages/AddUserPage';
import SuccessPage from './pages/SuccessPage';

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<SplashPage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/n/esams/main" element={<MainPage />} />
                <Route path="/n/esams/personnel" element={<PersonnelHomePage />} />
                <Route path="/n/esams/personnel/create" element={<AddUserPage />} />
                <Route path="/n/esams/personnel/success" element={<SuccessPage />} />
                <Route path="*" element={<Navigate to="/" />} />
            </Routes>
        </BrowserRouter>
    );
}