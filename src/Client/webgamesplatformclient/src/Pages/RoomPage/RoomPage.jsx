import { useParams, useNavigate} from "react-router-dom";
import { useSignalR } from "../../contexts/SignalRContext";
import { useAuth } from "../../contexts/AuthContext";
import { useEffect, useCallback, useState } from "react";
import { jwtDecode } from "jwt-decode";
const RoomPage = ()=>{
    const {roomId} = useParams();
    const {connect, isConnected, invokeMethod, onMethod} = useSignalR();
    const {getToken} = useAuth();
    const [IsRequestRecived, SetIsRequestRecived] = useState(false);
    const [IsSuccessResponse, SetIsSuccessResponse] = useState(false);
    const [RoomName, SetRoomName] = useState("");
    const [RoomMembers, SetRoomMembers] = useState([]);
    const [ShowStartButton, SetShowStartButton] = useState(false);
    const [RoomCreator, SetRoomCreator] = useState("");
    const navigate = useNavigate();
    const getRoom = useCallback(async () => {
        console.log("Get room method executed");
        console.log(String(roomId), typeof roomId);

       
        let response = await invokeMethod("GetRoomInformation", roomId);
        SetIsRequestRecived(true);

        console.log(response);
        if(response){
            SetIsSuccessResponse(true);
            if(response.isSuccess === true){
                SetRoomName(response.payload.room.roomName);
                SetRoomMembers(response.payload.members);
                SetRoomCreator(response.payload.room.creator);
            }
            else{
                navigate("/home");
            }
        }
        else{
            SetIsSuccessResponse(false);
        }
    }, [invokeMethod, roomId, navigate]);

    useEffect(() => {
        console.log("RoomPage mounted");
        if (!isConnected) {
            SetIsRequestRecived(false);
            console.log("Not connected, attempting to connect...");
            const token = getToken();
            connect(token);
        }
    }, [isConnected, connect, getToken]);
    
    const addRoomMember = useCallback((member)=>{
        console.log("AddRoomMember", member);
        SetRoomMembers((prevRoomMembers) => {
            if (!prevRoomMembers.includes(member)) {
                return [...prevRoomMembers, member];
            }
            return prevRoomMembers;
        });
    }, []);
    const removeRoomMember = useCallback((member)=>{
            console.log("RemoveRoomMember", member);
            SetRoomMembers((prevRoomMembers) =>
                prevRoomMembers.filter((m) => m !== member)
            );
    }, []);

    const onGameStarted = useCallback((sessionId)=>{
        console.log("Game started", sessionId);
        navigate("/session/"+sessionId);
    }, [navigate]);

    useEffect(() => {
        if (isConnected && !IsRequestRecived) {
            console.log("Connected, calling getRoom...");
            onMethod("AddRoomMember", (member)=>addRoomMember(member));
            onMethod("RemoveRoomMember", (member)=>removeRoomMember(member));
            onMethod("GameStarted", (sessionId)=>onGameStarted(sessionId));
            getRoom();
        }
    }, [isConnected, IsRequestRecived, getRoom, onMethod, RoomMembers, addRoomMember, removeRoomMember, onGameStarted]);

    useEffect(()=>{
        const jwtToken = getToken();
        const decoded = jwtDecode(jwtToken)
        if(decoded.sub && RoomCreator){
            if(decoded.sub == RoomCreator){
                SetShowStartButton(true);
            }
            else{
                SetShowStartButton(false);
            }
        }
        console.log(decoded);
    }, [getToken, RoomCreator]);

    const startGame = async ()=>{
        if(isConnected){
            let response = await invokeMethod("StartGame", roomId);
            console.log(response);
        }
    }


    return (
        <>
        {!isConnected? (<><h1>service unavailable</h1></>):(
            <>
        
                {IsRequestRecived? (
                <div className="page">
                    {IsSuccessResponse?(
                        <>
                            <h1>{RoomName}</h1>
                            <br/>
                            <h1>{RoomMembers}</h1>
                            {ShowStartButton?<button onClick={startGame}>Start Game</button>:<></>}    
                            
                        </>
                    ):(
                        <h1>service unavailable</h1>
                    )}
                </div>
                ):(
                    <h1>Please wait</h1>
                )}
            </>)}
        </>
    )
}
export default RoomPage;