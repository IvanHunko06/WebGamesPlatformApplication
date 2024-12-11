import { BrowserRouter as Router, Route, Routes, Navigate } from 'react-router-dom';
import Header from './components/Header/Header';
import Footer from './components/Footer/Footer';
import HomePage from './Pages/HomePage/HomePage';
import GamesPage from './Pages/GamesPage/GamesPage';
import LeaderPage from './Pages/LeaderPage/LeaderPage'; 
import JoinRoomPage from './Pages/JoinRoomPage/JoinRoomPage';
import { SignalRProvider } from './contexts/SignalRContext';
import './App.css'
import RoomPage from './Pages/RoomPage/RoomPage';
function MainApp(){
    return (
        <Router>
            <Header/>
            <SignalRProvider hubUrl="https://localhost:7005/api/hubs/session-managment-hub">
                <Routes>
                    <Route path="/join/:roomId" element={<JoinRoomPage/>}/>
                    <Route path="/room/:roomId" element={<RoomPage/>}/>
                </Routes>  
            </SignalRProvider>
            <Routes>
                <Route path="/" element={<Navigate to="/home" />} /> 
                <Route path="/home" element={<HomePage />} />
                <Route path="/leaderboard" element={<LeaderPage />} />
                <Route path="/games" element={<GamesPage />} /> 
            </Routes>
            <Footer/>
        </Router>
        

    );
}

export default MainApp;