import { useParams, useNavigate} from "react-router-dom";
import { useSignalR } from "../../contexts/SignalRContext";
import { useAuth } from "../../contexts/AuthContext";
import { useEffect, useCallback, useState } from "react";
import { jwtDecode } from "jwt-decode";
import axios from "axios";
import "./RoomPage.css";
import { useNotification } from "../../contexts/NotificationContext";

const RoomPage = () => {
    const { roomId } = useParams();
    const { connect, isConnected, invokeMethod, onMethod } = useSignalR();
    const { getToken } = useAuth();
    const [IsRequestRecived, SetIsRequestRecived] = useState(false);
    const [IsSuccessResponse, SetIsSuccessResponse] = useState(false);
    const [RoomName, SetRoomName] = useState("");
    const [RoomMembers, SetRoomMembers] = useState([]);
    const [ShowStartButton, SetShowStartButton] = useState(false);
    const [RoomCreator, SetRoomCreator] = useState("");
    const [notifications, setNotifications] = useState([]);
    const [MemberProfiles, SetMemberProfiles] = useState({});
    const { addNotification } = useNotification();
    
    const navigate = useNavigate();

    const getRoom = useCallback(async () => {
        console.log("Get room method executed");
        console.log(String(roomId), typeof roomId);

        let response = await invokeMethod("GetRoomInformation", roomId);
        SetIsRequestRecived(true);

        console.log("GetRoom"+response);
        if (response) {
            SetIsSuccessResponse(true);
            if (response.isSuccess === true) {
                SetRoomName(response.payload.room.roomName);
                SetRoomMembers(response.payload.members);
                SetRoomCreator(response.payload.room.creator);
            } else {
                navigate("/home");
            }
        } else {
            SetIsSuccessResponse(false);
        }
    }, [invokeMethod, roomId, navigate]);

    const fetchProfiles = useCallback(async (members) => {
        const token = await getToken();
        const profiles = {};

        await Promise.all(members.map(async (member) => {
            try {
                const response = await axios.get(
                    `https://localhost:7005/api/services/profile-service/rest/GetProfile/${member}`,
                    {
                        headers: { Authorization: `Bearer ${token}` }
                    }
                );
                profiles[member] = {
                    name: response.data.publicName || member,
                    image: response.data.smallImageUrl || ""
                };
            } catch (error) {
                console.error(`Error fetching profile for ${member}:`, error);
                profiles[member] = { name: member, image: "" };
            }
        }));

        SetMemberProfiles(profiles);
    }, [getToken]);

    useEffect(() => {
        if (RoomMembers.length > 0) {
            fetchProfiles(RoomMembers);
        }
    }, [RoomMembers, fetchProfiles]);

    useEffect(() => {
        console.log("RoomPage mounted");
        if (!isConnected) {
            SetIsRequestRecived(false);
            console.log("Not connected, attempting to connect...");
            const token = getToken();
            connect(token);
        }
    }, [isConnected, connect, getToken]);
    
    const addRoomMember = useCallback((member) => {
        console.log("AddRoomMember", member);

        addNotification(`Player ${member} joined the room`);

        SetRoomMembers((prevRoomMembers) => {
            if (!prevRoomMembers.includes(member)) {
                return [...prevRoomMembers, member];
            }
            return prevRoomMembers;
        });
    }, []);

    const removeRoomMember = useCallback((member) => {
        console.log("RemoveRoomMember", member);

        addNotification(`Player ${member} left the room`);

        SetRoomMembers((prevRoomMembers) =>
            prevRoomMembers.filter((m) => m !== member)
        );
    }, []);

    const onGameStarted = useCallback((sessionId) => {
        console.log("Game started", sessionId);
        // setNotifications((prev) => [...prev, "Game started"]);
        navigate("/session/" + sessionId);
    }, [navigate]);

    useEffect(() => {
        if (isConnected && !IsRequestRecived) {
            console.log("Connected, calling getRoom...");
            onMethod("AddRoomMember", (member) => addRoomMember(member));
            onMethod("RemoveRoomMember", (member) => removeRoomMember(member));
            onMethod("GameStarted", (sessionId) => onGameStarted(sessionId));
            getRoom();
        }
    }, [isConnected, IsRequestRecived, getRoom, onMethod, RoomMembers, addRoomMember, removeRoomMember, onGameStarted]);

    useEffect(() => {
        const jwtToken = getToken();
        const decoded = jwtDecode(jwtToken);
        console.log(decoded);
        if (decoded.preferred_username && RoomCreator) {
            if (decoded.preferred_username == RoomCreator) {
                SetShowStartButton(true);
            } else {
                SetShowStartButton(false);
            }
        }
    }, [getToken, RoomCreator]);

    const startGame = async () => {
        if (isConnected) {
            let response = await invokeMethod("StartGame", roomId);
            console.log("response", response);
    
            if (!response.isSuccess) {
                addNotification(`Error: ${response.errorMessage}`, "error");
            }
        }
    };
    
    return (
        <>
            {!isConnected ? (
                <h1>Service unavailable</h1>
            ) : (
                <div className="room-container">
                    {IsRequestRecived ? (
                        IsSuccessResponse ? (
                            <>
                                <h1 className="room-title">{RoomName}</h1>
                                {ShowStartButton && (
                                    <button className="start-game-button" onClick={startGame}>
                                        Start Game
                                    </button>
                                )}
                                <div className="members-container">
                                    {RoomMembers.map((member, index) => (
                                        <div className="member-tile" key={index}>
                                            <div className="member-image">
                                                <img src={MemberProfiles[member]?.image || "https://localhost:7005/resources/images/no-image-profile.png"} alt={MemberProfiles[member]?.name} />
                                            </div>
                                            <span className="member-name">{MemberProfiles[member]?.name || member}</span>
                                        </div>
                                    ))}
                                </div>

                            </>
                        ) : (
                            <h1>Service unavailable</h1>
                        )
                    ) : (
                        <h1>Please wait...</h1>
                    )}
                </div>
            )}
        </>
    );
    
};

export default RoomPage;
