import { useEffect, useRef } from 'react';
import { useLocation } from 'react-router-dom';
import { useSignalR } from '../contexts/SignalRContext';

function RouteChangeHandler() {
    const location = useLocation();
    const { invokeMethod, disconnect } = useSignalR();
    const previousLocation = useRef(location.pathname);

    const getSavedRoomId = () => localStorage.getItem('savedRoomId');
    const setSavedRoomId = (roomId) => {
        if (roomId) {
            localStorage.setItem('savedRoomId', roomId);
        } 
    };

    useEffect(() => {
        const isRoomPage = (path) =>
            path.startsWith('/join/') || path.startsWith('/room/') || path.startsWith('/session/');

        const extractRoomId = (path) => {
            if (path.startsWith('/join/')) {
                return path.split('/')[2]; 
            }
            return null;
        };

        const handleLeaveRoom = async (roomId) => {
            if (roomId) {
                console.log(`Leaving room: ${roomId}`);
                await invokeMethod("LeaveRoom", roomId);
                disconnect();
                setSavedRoomId(null);
            }
            else
            console.log(`Leaving room: NULL`);
        };

        if (location.pathname.startsWith('/join/')) {
            const roomId = extractRoomId(location.pathname);
            if (roomId && roomId !== getSavedRoomId()) {
                setSavedRoomId(roomId);
            }
        }

        if (
            isRoomPage(previousLocation.current) && 
            !isRoomPage(location.pathname)
        ) {
            console.log(`Leaving room: ${getSavedRoomId()}`);
            handleLeaveRoom(getSavedRoomId());
        }

        if (isRoomPage(location.pathname) && getSavedRoomId() !== extractRoomId(location.pathname)) {
            const newRoomId = extractRoomId(location.pathname);
            if (newRoomId) {
                setSavedRoomId(newRoomId);
            }
        }

        previousLocation.current = location.pathname;
    }, [location, invokeMethod, disconnect]);

    return null;
}

export default RouteChangeHandler;
