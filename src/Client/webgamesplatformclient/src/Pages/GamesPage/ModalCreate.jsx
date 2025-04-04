import React, { useState, useEffect } from "react";
import axios from "axios";
import { useAuth } from "../../contexts/AuthContext";
import { NavLink } from "react-router-dom";
import 'font-awesome/css/font-awesome.min.css';
import { useNotification } from "../../contexts/NotificationContext";


const ModalCreate = ({ showModal, onClose, game }) => {
  const [name, setName] = useState("");
  const [isNameValid, setIsNameValid] = useState(true);
  const [isPublic, setIsPublic] = useState(false);
  const [players, setPlayers] = useState(game?.minPlayers);
  const [roomData, setRoomData] = useState(null);
  const { addNotification } = useNotification();
  const [loading, setLoading] = useState(false);

  const { getToken } = useAuth();

  const handleModalClick = (e) => {
    e.stopPropagation();
  };

  const handleNameChange = (e) => {
    setName(e.target.value);
    setIsNameValid(true);
  };

  const handleToggle = () => {
    setIsPublic((prev) => !prev);
  };

  const handleSliderChange = (e) => {
    setPlayers(Number(e.target.value));
  };

  const handleCreate = async () => {
    if (!name.trim()) {
      setIsNameValid(false);
      return;
    }

    setLoading(true);
    try {
      const token = await getToken();
      const response = await axios.post(
        "https://localhost:7005/api/services/rooms-service/rest/CreateRoom",
        {
          RoomName: name.trim(),
          IsPrivate: !isPublic,
          SelectedPlayersCount: players,
          GameId: game?.id,
        },
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      console.log("Room Created:", response.data);
      setLoading(false);

      setRoomData(response.data);  
    } catch (error) {
      addNotification(`Failed to create room`, "error");
      setLoading(false);
    }
  };

  const handleCopy = (text,token) => {
    navigator.clipboard.writeText("http://localhost:5173/join/"+text+"?accessToken="+token).then(
      () => {
        addNotification("Text copied to clipboard");
      },
      (err) => {
        // console.error("Failed to copy text: ", err);
        addNotification("Failed to copy text","error");
      }
    );
  };

  useEffect(() => {
    if (showModal) {
      setName("");
      setIsNameValid(true);
      setIsPublic(true);
      setPlayers(game?.minPlayers);
      setRoomData(null); 
    }
  }, [showModal, game]);

  if (!showModal) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        {loading ? (
          <h2>Loading...</h2>
        ) : roomData ? (
          <div>
            <h2>Room Created Successfully</h2>
            <p className="modal-copy">Room ID: {roomData.roomId}</p>
            {roomData.accessToken && (
              <p className="modal-copy">Access Token: {roomData.accessToken}</p>
            )}
            <button className="modal-continue" onClick={() => handleCopy(roomData.roomId, roomData.accessToken)}>
              <i className="fa fa-clipboard" aria-hidden="true"></i>
            </button>
            <NavLink className="modal-continue" to={`/join/${roomData.roomId}?accessToken=${roomData.accessToken}`}>
              Go to Room
            </NavLink>
          </div>
        ) : (
          <div>
            <h2>Create Room for {game?.name || "Game"}</h2>
            <input
              type="text"
              placeholder="Enter room name"
              className={`modal-input ${!isNameValid ? "invalid" : ""}`}
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
            <div className="toggle-container" onClick={() => setIsPublic((prev) => !prev)}>
              <div className={`toggle ${isPublic ? "public" : "private"}`}>
                <div className="toggle-circle"></div>
              </div>
              <span className="toggle-text">{isPublic ? "Public" : "Private"}</span>
            </div>
            <div className="slider-container">
              <label className="slider-label">Players: {players}</label>
              {game?.minPlayers !== game?.maxPlayers && (
                <input
                  type="range"
                  min={game?.minPlayers}
                  max={game?.maxPlayers}
                  step="1"
                  value={players}
                  onChange={(e) => setPlayers(Number(e.target.value))}
                  className="slider"
                />
              )}
            </div>
            <button className="modal-continue" onClick={handleCreate}>
              Create
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default ModalCreate;
