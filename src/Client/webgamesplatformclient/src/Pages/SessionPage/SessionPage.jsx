import React, { useCallback, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useSignalR } from "../../contexts/SignalRContext";
import { useAuth } from "../../contexts/AuthContext";
import { GameProvider } from "../../contexts/GameContext";
import TicTacToe from "../../components/Games/TicTacToe/TicTacToe";
import "./SessionPage.css";
import { NavLink } from "react-router-dom";

const SessionPage = () => {
    const { sessionId } = useParams();
    const { connect, isConnected, invokeMethod, onMethod, disconnect } = useSignalR();
    const { getToken } = useAuth();
    const [needReconnect, setNeedReconnect] = useState(true);
    const [gameId, setGameId] = useState("inProgress");
    const [modalInfo, setModalInfo] = useState(null);

    const getSessionInfo = useCallback(async () => {
        let response = await invokeMethod("GetSessionInformation", sessionId);
        if (response?.isSuccess && response.payload) {
            setGameId(response.payload.gameId);
        }
    }, [invokeMethod, sessionId]);

    useEffect(() => {
        if (!isConnected) {
            connect(getToken());
        }
    }, [isConnected, connect, getToken]);

    useEffect(() => {
        if (isConnected && needReconnect) {
            getSessionInfo();
        }
    }, [isConnected, getSessionInfo, needReconnect]);

    const sessionEnded = useCallback((reason, payload) => {
        let modalData;
        if (reason === "NORMAL_FINISH") {
            const isWin = !payload.includes("-");
            modalData = {
                title: isWin ? "Victory!" : "Defeat",
                colorClass: isWin ? "modal-title-green" : "modal-title-red",
                points: payload,
                animationClass: isWin ? "modal-title-win" : "modal-title-loss",
            };
        } else if (reason === "PLAYER_DISCONNECTED") {
            modalData = {
                title: "Player Disconnected",
                colorClass: "modal-title-gray",
                points: "0",
                animationClass: "modal-title-neutral",
            };
        }
        setModalInfo(modalData);
    }, []);

    const closeConnection = useCallback((reason) => {
        disconnect();
        setNeedReconnect(false);
    }, [disconnect]);

    useEffect(() => {
        onMethod("SessionEnded", (reason, payload) => sessionEnded(reason, payload));
        onMethod("CloseConnection", (reason) => closeConnection(reason));
    }, [onMethod, sessionEnded, closeConnection]);

    const getGameComponent = useCallback(() => {
        if (gameId === "tictactoe") return <TicTacToe />;
        if (gameId === "inProgress") return <h1>Please wait</h1>;
        return <h1>Unknown gameId</h1>;
    }, [gameId]);

    return (
        <>
            <GameProvider sessionId={sessionId}>{getGameComponent()}</GameProvider>
            {modalInfo && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <h1 className={`modal-title ${modalInfo.colorClass} ${modalInfo.animationClass}`}>{modalInfo.title}</h1>
                        <p className="modal-points">{modalInfo.points} Points</p>
                        <NavLink to="/home" className="modal-continue">
                            Continue
                        </NavLink>
                    </div>
                </div>
            )}
        </>
    );
};

export default SessionPage;
