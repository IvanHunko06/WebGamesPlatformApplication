import { useParams, useSearchParams, useNavigate} from "react-router-dom";
import { useSignalR } from "../../contexts/SignalRContext";
import { useAuth } from "../../contexts/AuthContext";
import { useEffect, useState,  useCallback} from "react";
const JoinRoomPage = ()=>{
    const {roomId} = useParams();
    const [searchParams] = useSearchParams();
    const {connect, isConnected, invokeMethod} = useSignalR();
    const [IsRequestRecived, SetIsRequestRecived] = useState(false);
    const [IsSuccessResponse, SetIsSuccessResponse] = useState(false);
    const navigate = useNavigate();
    const {getToken} = useAuth();
    const accessToken = searchParams.get("accessToken");
    const joinRoom = useCallback(async () => {
        console.log("Join room method executed");
        console.log(String(roomId), typeof roomId);
        console.log(String(accessToken), typeof accessToken);

        let response = {
            returnValue: undefined,
            isSuccess: false,
        };
        await invokeMethod("JoinRoom", response, roomId, accessToken ?? "");
        SetIsRequestRecived(true);

        console.log(response);
        if (response.isSuccess === true && response.returnValue) {
            SetIsSuccessResponse(true);
            if (response.returnValue.isSuccess === true) {
                navigate("/room/" + roomId, { state: { connectionLink: window.location.href, reJoinLocalPath: window.location.pathname } });
            } else if (response.returnValue.isSuccess === false && response.returnValue.errorMessage === "ALREADY_IN_ROOM") {
                navigate("/home");
            } else {
                navigate("/home");
            }
        } else {
            SetIsSuccessResponse(false);
        }
    }, [invokeMethod, roomId, accessToken, navigate]);


    useEffect(() => {
        console.log("JoinRoomPage mounted");
        if (!isConnected) {
            SetIsRequestRecived(false);
            console.log("Not connected, attempting to connect...");
            const token = getToken();
            connect(token);
        }
    }, [isConnected, connect, getToken]);
    
    useEffect(() => {
        if (isConnected && !IsRequestRecived) {
            console.log("Connected, calling joinRoom...");
            joinRoom();
        }
    }, [isConnected, IsRequestRecived, joinRoom]);

    return(
        <>
        {!isConnected? (<><h1>service unaviliable</h1></>):(
            <>
        
                {IsRequestRecived? (
                <>
                    {IsSuccessResponse?(
                        <></>
                    ):(
                        <h1>service unaviliable</h1>
                    )}
                </>
                ):(
                    <h1>Please wait</h1>
                )}
            </>)}
        </>
    )
}
export default JoinRoomPage;