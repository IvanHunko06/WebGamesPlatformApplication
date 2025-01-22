import { useState, useEffect, useCallback } from "react";
import { useGame } from "../../../contexts/GameContext";
import { jwtDecode } from "jwt-decode";
import { useAuth } from "../../../contexts/AuthContext";
import "./TicTacToe.css";

const TicTacToe = () => {
  const [board, setBoard] = useState(Array(9).fill(null));
  const [PlayerId, SetPlayerId] = useState(undefined);
  const [CurrentPlayer, SetCurrentPlayer] = useState(undefined);
  const [XPlayer, SetXPlayer] = useState(undefined);
  const [OPlayer, SetOPlayer] = useState(undefined);
  const { sendAction, syncGameState, setCallbackFunction, sessionId } = useGame();
  const { getToken } = useAuth();

  const handleClick = async (index) => {
    console.log("Click on", index);
    const payload = {
      CellId: index
    };
    let reply = await sendAction(sessionId, "PUT", JSON.stringify(payload));
    console.log(reply);
  };

  const reciveAction = useCallback((action) => {
    const obj = JSON.parse(action);
    console.log("ReciveActions", obj);
    for (let i = 0; i < obj.length; ++i) {
      console.log("Current message", obj[i]);
      if (obj[i].Action === "UpdateGameboard") {
        const parsedGameBoard = JSON.parse(obj[i].Payload);
        setBoard(parsedGameBoard);
      } else if (obj[i].Action === "UpdateCurrentPlayer") {
        SetCurrentPlayer(obj[i].Payload);
      }
    }
  }, []);

  const syncGameStateProccess = useCallback(async () => {
    let reply = await syncGameState(sessionId);
    console.log("SyncGameState", reply);
    if (reply.isSuccess === true && reply.payload) {
      const parsedPayload = JSON.parse(reply.payload);
      console.log(parsedPayload);
      setBoard(parsedPayload.GameBoard);
      SetCurrentPlayer(parsedPayload.CurrentPlayer);
      SetXPlayer(parsedPayload.XPlayer); 
      SetOPlayer(parsedPayload.OPlayer); 
    }
  }, [sessionId, syncGameState]);

  useEffect(() => {
    setCallbackFunction(reciveAction);
    const interval = setInterval(() => {
      syncGameStateProccess();
    }, 5 * 60 * 1000);
    syncGameStateProccess();
    return () => {
      clearInterval(interval);
    };
  }, [setCallbackFunction, reciveAction, syncGameStateProccess]);

  useEffect(() => {
    const jwtToken = getToken();
    const decoded = jwtDecode(jwtToken);
    if (decoded.preferred_username) {
      SetPlayerId(decoded.preferred_username);
    }
  }, [getToken]);

  const currentPlayerSymbol = CurrentPlayer === XPlayer ? "X" : "O";
  const opponentSymbol = CurrentPlayer === XPlayer ? "O" : "X";

  return (
    <div className="game-container">
      <div className="player-status">
        {CurrentPlayer === PlayerId ? (
          <span>Your Move ({currentPlayerSymbol})</span>
        ) : (
          <span>Opponent's Move ({opponentSymbol})</span>
        )}
      </div>
      <h1>Tic Tac Toe</h1>
      <div className="player-info">
        {XPlayer === PlayerId ? (
          <>
            <p>{XPlayer} is X</p>
            <p>{OPlayer} is O</p>
          </>
        ) : (
          <>
            <p>{OPlayer} is O</p>
            <p>{XPlayer} is X</p>
          </>
        )}
      </div>
      <div className="board">
        {board.map((cell, index) => (
          <div
            key={index}
            className="cell"
            onClick={() => handleClick(index)}
          >
            {cell}
          </div>
        ))}
      </div>
    </div>
  );
};

export default TicTacToe;
