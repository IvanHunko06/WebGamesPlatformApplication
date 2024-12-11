import { useParams, useNavigate, useLocation} from "react-router-dom";
import { useSignalR } from "../../contexts/SignalRContext";
import { useAuth } from "../../contexts/AuthContext";
import { useEffect, useCallback, useState } from "react";
const RoomPage = ()=>{
    const {roomId} = useParams();
    const {connect, isConnected, invokeMethod, onMethod} = useSignalR();
    const {getToken} = useAuth();
    const [IsRequestRecived, SetIsRequestRecived] = useState(false);
    const [IsSuccessResponse, SetIsSuccessResponse] = useState(false);
    const [RoomName, SetRoomName] = useState("");
    const [RoomMembers, SetRoomMembers] = useState([]);
    const location = useLocation();
    const { connectionLink, reJoinLocalPath} = location.state || {};
    const navigate = useNavigate();
    const getRoom = useCallback(async () => {
        console.log("Get room method executed");
        console.log(String(roomId), typeof roomId);

        let response = {
            returnValue: undefined,
            isSuccess: false,
        };
        await invokeMethod("GetRoomInformation", response, roomId);
        SetIsRequestRecived(true);

        console.log(response);
        if(response.isSuccess === true && response.returnValue){
            SetIsSuccessResponse(true);
            if(response.returnValue.isSuccess === true){
                SetRoomName(response.returnValue.room.roomName);
                SetRoomMembers(response.returnValue.members);
            }
            else if(response.returnValue.isSuccess === false && response.returnValue.errorMessage == "NOT_ALLOWED"){
                //console.log("NOT_ALLOWED. Navigate to ", reJoinLocalPath)
                //navigate(reJoinLocalPath);
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

    useEffect(() => {
        if (isConnected && !IsRequestRecived) {
            console.log("Connected, calling getRoom...");
            onMethod("AddRoomMember", (member)=>addRoomMember(member));
            onMethod("RemoveRoomMember", (member)=>removeRoomMember(member));
            getRoom();
        }
    }, [isConnected, IsRequestRecived, getRoom, onMethod, RoomMembers, addRoomMember, removeRoomMember]);



    return (
        <>
        {!isConnected? (<><h1>service unavailable</h1></>):(
            <>
        
                {IsRequestRecived? (
                <div className="page">
                    {IsSuccessResponse?(
                        <>
                            <h1>{connectionLink}</h1>
                            <h1>{RoomName}</h1>
                            <br/>
                            <h1>{RoomMembers}</h1>
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