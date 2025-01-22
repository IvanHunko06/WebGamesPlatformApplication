import { useParams } from "react-router-dom";
import { useSignalR } from "../../contexts/SignalRContext";
import { useEffect, useCallback, useState } from "react";
import TicTacToe from "../../components/Games/TicTacToe/TicTacToe";
import { useAuth } from "../../contexts/AuthContext";
import { GameProvider } from "../../contexts/GameContext";
const SessionPage = ()=>{
    const {sessionId} = useParams();
    const {connect, isConnected, invokeMethod, onMethod, disconnect} = useSignalR();
    const [needReconnect, setNeedReconnect] = useState(true);
    const [gameId, setGameId] = useState("inProgress");
    const {getToken} = useAuth();
    const getSessionInfo = useCallback(async () => {
            console.log("Get session info executed", String(sessionId), typeof roomId);
            let response = await invokeMethod("GetSessionInformation", sessionId);
            if(response){
                if(response.isSuccess && response.payload){
                    setGameId(response.payload.gameId);
                }
            }
            console.log(response);
        }, [invokeMethod, sessionId]);
    useEffect(() => {
        console.log("SessionPage mounted");
        if (!isConnected) {
            console.log("Not connected, attempting to connect...");
            const token = getToken();
            connect(token);
        }
    }, [isConnected, connect, getToken]);
    useEffect(() => {
        if (isConnected && needReconnect) {
            console.log("Connected, calling getSessionInfo...");
            getSessionInfo();
        }
    }, [isConnected, getSessionInfo]);
    let getGameComponent = useCallback(()=>{
        if(gameId === "tictactoe")
            return <TicTacToe/>;
        else if(gameId === "inProgress")
            return <h1>Please wait</h1>
        else
            return <h1>unknown gameId</h1>;
    }, [gameId])
    const sessionEnded = useCallback((reason, payload)=>{
        console.log("Game ended.", reason, payload);
    }, []);
    const closeConnection = useCallback((reason)=>{
        console.log("Closing connection with reason", reason);
        disconnect();
        setNeedReconnect(false);
    }, [])
    useEffect(()=>{
        onMethod("SessionEnded", (reason, payload)=>sessionEnded(reason, payload));
        onMethod("CloseConnection", (reason)=>closeConnection(reason));
    }, [onMethod])
    
    return(
        <>
        <GameProvider sessionId={sessionId}>
        {getGameComponent()}
        </GameProvider>
        </>
    )
}
export default SessionPage;