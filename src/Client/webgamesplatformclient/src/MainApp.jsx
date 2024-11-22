import { BrowserRouter as Router, Route, Routes, Navigate } from 'react-router-dom';
import './App.css'
import { SignalRProvider } from './contexts/SignalRContext'
import { useAuth } from './contexts/AuthContext';
import HomePage from './Сomponents/HomePage';
import Header from './Сomponents/Header';
import Footer from './Сomponents/Footer';
import LeaderPage from './Сomponents/LeaderPage';
import GamesPage from './Сomponents/GamesPage';
function MainApp(){
    const hubUrlOne = 'https://localhost:7005/api/hubs/main-hub';
    const {logout, getToken} = useAuth()
    let token = getToken()
    return (
        <SignalRProvider hubUrl={hubUrlOne} jwtToken={token}>
            <Header/>
            <Routes>
                <Route path="/home" element={<HomePage />} />
                <Route path="/leaderboard" element={<LeaderPage />} />
                <Route path="/games" element={<GamesPage />} /> 
                <Route path="*" element={<Navigate to="/home" />} />
            </Routes>
            <Footer/>
        </SignalRProvider>
    );
}

export default MainApp;