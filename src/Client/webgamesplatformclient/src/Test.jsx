import { useSignalR } from "./contexts/SignalRContext";
import { useEffect } from "react";
function Test(){
    const {isConnected, connection} = useSignalR();
    useEffect(()=>{
        if(isConnected){
            connection.current.on("AddRoom", (room)=>{
                console.log("AddRoom called from server");
                console.log(room);
            });
            connection.current.on("RemoveRoom", (roomId)=>{
                console.log("RemoveRoom called from server");
                console.log(roomId);
            });
            connection.current.invoke("ListenRooms", "tictactoe").then(result=>{
                console.log("ListenRooms method invoked");
                console.log(result);
            })
        }

    }, [isConnected, connection])

    return (
        <>
        <h1>TestComponent</h1>
        </>
    )
}
export default Test;