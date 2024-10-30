import {createContext, useContext, useRef, useEffect, useState, useMemo } from 'react';
import * as signalR from '@microsoft/signalr';
const SignalRContext = createContext(undefined);

// eslint-disable-next-line react/prop-types
export const SignalRProvider = ({hubUrl,jwtToken, children}) =>{
    let connection = useRef(null);
    const [isConnected, setIsConnected] = useState(false);
    useEffect(()=>{
        if(!hubUrl) return;
        const connect = async () =>{
            const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                withCredentials: true,
                accessTokenFactory: ()=>jwtToken,
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

            try{
                await newConnection.start();
                console.log(`SignalR connection established with ${hubUrl}`);
                connection.current = newConnection;
                setIsConnected(true);
            }catch(error){
                console.error('SignalR connection error:', error);
            }
        };
        if(!connection.current){
            connect();
        }

        return () =>{
            if(connection.current){
                connection.current.stop();
                console.log('SignalR connection stopped');
                setIsConnected(false);
            }
        }

       

    },[hubUrl, jwtToken]);

    let signalRContext = useMemo(()=>({
        isConnected,
        connection
    }), [isConnected]); 

    return (
        <SignalRContext.Provider value={signalRContext}>
            {children}
        </SignalRContext.Provider>
    );

};
// eslint-disable-next-line react-refresh/only-export-components
export const useSignalR = () => useContext(SignalRContext);