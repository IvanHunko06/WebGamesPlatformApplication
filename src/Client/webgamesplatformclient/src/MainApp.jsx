import { BrowserRouter as Router, Route, Routes, Navigate } from 'react-router-dom';
import Header from './components/Header/Header';
import Footer from './components/Footer/Footer';
import HomePage from './Pages/HomePage/HomePage';
import GamesPage from './Pages/GamesPage/GamesPage';
import LeaderPage from './Pages/LeaderPage/LeaderPage'; 
import ProfilePage from './Pages/ProfilePage/ProfilePage'; 
import JoinRoomPage from './Pages/JoinRoomPage/JoinRoomPage';
import { SignalRProvider } from './contexts/SignalRContext';
import './App.css'
import './components/Header/Header.css'
import { useAuth } from './contexts/AuthContext';
import { useJwt } from './contexts/JwtTokenContext';
import RoomPage from './Pages/RoomPage/RoomPage';
import SessionPage from './Pages/SessionPage/SessionPage';
import RoomsList from './Pages/RoomsListPage/RoomsListPage';
import RouteChangeHandler from './Pages/RouteChangeHandler';
import MatchHistoryPage from './Pages/MatchHistoryPage/MatchHistoryPage';
function MainApp(){
    const { logout } = useAuth();
    const { getUsername } = useJwt();
 
    return (
        <Router>
            <Header logout={logout} name={getUsername()} /> 
            <SignalRProvider hubUrl="https://localhost:7005/api/hubs/session-managment-hub">
                <RouteChangeHandler/>
                <Routes>
                    <Route path="/join/:roomId" element={<JoinRoomPage/>}/>
                    <Route path="/room/:roomId" element={<RoomPage/>}/>
                    <Route path="/session/:sessionId" element={<SessionPage/>}/>
                </Routes>  
            </SignalRProvider>
            <Routes>
                <Route path="/" element={<Navigate to="/home" />} /> 
                <Route path="/home" element={<HomePage />} />
                <Route path="/leaderboard" element={<LeaderPage />} />
                <Route path="/games" element={<GamesPage />} /> 
                <Route path="/profile/:username" element={<ProfilePage />} /> 
                <Route path="/rooms-list/:gameId" element={<RoomsList />} /> 
                <Route path="/match-history/:username" element={<MatchHistoryPage />} /> 
            </Routes>
            <Footer/>
        </Router>
    );
}

export default MainApp;