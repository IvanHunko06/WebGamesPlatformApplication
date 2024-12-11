import {createContext, useContext, useState, useMemo, useCallback , useEffect} from 'react';
import * as signalR from '@microsoft/signalr';
const SignalRContext = createContext(undefined);

// eslint-disable-next-line react/prop-types
export const SignalRProvider = ({hubUrl, children}) =>{
    //let connection = useRef(null);
    const [isConnected, setIsConnected] = useState(false);
    const [connection, setConnection] = useState();
    useEffect(()=>{
        console.log("SignalRProvider useEffect hook");
        return ()=>{
            console.log("SignalRProvider useEffect hook. unmount function");
            disconnect();
        }
    })

    const connect = useCallback(async (jwtToken)=>{
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                withCredentials: true,
                accessTokenFactory: ()=>jwtToken,
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();
            console.log("Attempt to start new connection")
            try{
                await newConnection.start();
                console.log(`SignalR connection established with ${hubUrl}`);
                setIsConnected(true);
                setConnection(newConnection);
            }catch(error){
                setIsConnected(false);
                setConnection();
                console.error('SignalR connection error:', error);
            }
            newConnection.onclose(() => {
                console.warn('SignalR connection closed.');
                setIsConnected(false);
              });
          
            newConnection.onreconnecting(() => {
                console.warn('Reconnecting to SignalR hub...');
                setIsConnected(false);
            });
        
            newConnection.onreconnected(() => {
                console.log('Reconnected to SignalR hub');
                setIsConnected(true);
            });

    }, [hubUrl]);

    const disconnect = useCallback(()=>{
        if(isConnected){
            connection.stop();
        }
    }, [connection, isConnected])

    const invokeMethod = useCallback(async (methodName, response, ...args)=>{
        if(isConnected){
            console.log("Invoke method", methodName);
            console.log("Args: ", args);
            try{
                
                response.returnValue = await connection.invoke(methodName, ...args);
                response.isSuccess = true;
                
            }catch(error){
                response.isSuccess = false;
                console.error('SignalR connection error:', error);
            }

        }
        else{
            response.isSuccess = false;
        }
    }, [connection, isConnected]);

    const onMethod = useCallback((methodName, handler)=>{
        if(isConnected){
            try{
                connection.on(methodName, handler);
            }catch(error){
                console.error('SignalR connection error:', error);
            }

        }
    }, [connection, isConnected]);
    let signalRContext = useMemo(()=>({
        connect,
        disconnect,
        onMethod,
        invokeMethod,
        isConnected,
    }), [isConnected, onMethod, invokeMethod, connect, disconnect]); 

    return (
        <SignalRContext.Provider value={signalRContext}>
            {children}
        </SignalRContext.Provider>
    );

};
// eslint-disable-next-line react-refresh/only-export-components
export const useSignalR = () => useContext(SignalRContext);