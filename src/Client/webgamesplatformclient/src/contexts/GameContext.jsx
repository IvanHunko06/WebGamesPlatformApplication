import { createContext, useContext,useCallback, useRef, useEffect } from "react";
import { useSignalR } from "./SignalRContext";
export const GameContext = createContext();

// eslint-disable-next-line react/prop-types
export const GameProvider = ({ children, sessionId }) => {
    const {isConnected, invokeMethod, onMethod} = useSignalR();
    const serverResponseCallbackFunction = useRef(undefined);
    const sendAction = useCallback(async (sessionId, actionName, payload)=>{
        if(isConnected){
            let returnValue = await invokeMethod("MakeAction", sessionId , actionName, payload)
            return returnValue;
        }
    }, [isConnected, invokeMethod])
  
    const syncGameState = useCallback(async (sessionId)=>{
        if(isConnected){
            let returnValue = await invokeMethod("SyncGameState", sessionId)
            return returnValue;
        }
    }, [isConnected, invokeMethod])
  
    const setCallbackFunction = useCallback((callbackFunction)=>{
        serverResponseCallbackFunction.current = callbackFunction;
    }, []);
    const invokeCallbackFunction = useCallback((action)=>{
        serverResponseCallbackFunction.current(action);
    }, [serverResponseCallbackFunction]);
    useEffect(()=>{
        onMethod("ReciveAction", invokeCallbackFunction);
    }, [invokeCallbackFunction, onMethod]);
    
  
    return (
      <GameContext.Provider value={{ sendAction, syncGameState, setCallbackFunction, sessionId }}>
        {children}
      </GameContext.Provider>
    );
  };
  // eslint-disable-next-line react-refresh/only-export-components
  export const useGame = () => useContext(GameContext);