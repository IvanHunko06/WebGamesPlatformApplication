import { useState, useEffect, useCallback } from "react";
import { useGame } from "../../../contexts/GameContext";
import { jwtDecode } from "jwt-decode";
import { useAuth } from "../../../contexts/AuthContext";
const TicTacToe = ()=>{
    const [board, setBoard] = useState(Array(9).fill(null));
    const [PlayerId, SetPlayerId] = useState(undefined);
    const [CurrentPlayer, SetCurrentPlayer] = useState(undefined);
    const {sendAction, syncGameState, setCallbackFunction, sessionId } = useGame();
    const {getToken} = useAuth();
    const handleClick = async (index) => {
      //if (board[index]) return;
      console.log("Click on", index);
      const payload = {
        CellId : index
      }
      let reply = await sendAction(sessionId, "PUT", JSON.stringify(payload));
      console.log(reply);
      //const newBoard = [...board];
      //newBoard[index] = isXNext ? "X" : "O";
      //setBoard(newBoard);
      //setIsXNext(!isXNext);
    };

    const reciveAction = useCallback((action)=>{
      const obj = JSON.parse(action);
      console.log("ReciveAction", obj);
      const payload = JSON.parse(obj.Payload);
      console.log("Parsed payload", payload);
      for(let i = 0; i < payload.length; ++i){
        console.log("Current message", payload[i])
        if(payload[i].Action === "UpdateGameboard"){
          const parsedGameBoard = JSON.parse(payload[i].Payload);
          setBoard(parsedGameBoard);
        }
        else if(payload[i].Action === "UpdateCurrentPlayer"){
          SetCurrentPlayer(payload[i].Payload);
        }
      }
    }, []);

    const syncGameStateProccess = useCallback(async ()=>{
      let reply = await syncGameState(sessionId);
      console.log("SyncGameState", reply);
      if(reply.isSuccess === true && reply.payload){
        const parsedPayload = JSON.parse(reply.payload);
        console.log(parsedPayload);
        setBoard(parsedPayload.GameBoard)
        SetCurrentPlayer(parsedPayload.CurrentPlayer);
      }

      
    }, [sessionId, syncGameState]);

    useEffect(()=>{
      setCallbackFunction(reciveAction)
      const interval = setInterval(()=>{
        syncGameStateProccess();
      }, 5 * 60 * 1000);
      syncGameStateProccess();
      return ()=>{
        clearInterval(interval);
      }
    }, [setCallbackFunction, reciveAction, syncGameStateProccess]);

    useEffect(()=>{
      const jwtToken = getToken();
      const decoded = jwtDecode(jwtToken)
      if(decoded.sub){
        SetPlayerId(decoded.sub)
      }
    }, [getToken])
    return (
      <div>
        {CurrentPlayer === PlayerId?<h1>Your Move</h1>:<h1>opponents move</h1>}
        <h1>Tic Tac Toe</h1>
        <div style={styles.board}>
          {board.map((cell, index) => (
            <div
              key={index}
              style={styles.cell}
              onClick={() => handleClick(index)}
            >
              {cell}
            </div>
          ))}
        </div>
      </div>
    );
}
const styles = {
    board: {
      display: "grid",
      gridTemplateColumns: "repeat(3, 100px)",
      gridTemplateRows: "repeat(3, 100px)",
      gap: "5px",
    },
    cell: {
      width: "100px",
      height: "100px",
      display: "flex",
      justifyContent: "center",
      alignItems: "center",
      fontSize: "24px",
      border: "1px solid black",
      cursor: "pointer",
    },
  };
export default TicTacToe;