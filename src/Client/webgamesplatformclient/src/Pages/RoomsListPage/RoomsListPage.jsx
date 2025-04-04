import React, { useState, useEffect } from "react";
import { useParams, NavLink } from "react-router-dom";
import axios from "axios";
import "./RoomsListPage.css";
import ModalJoin from "../GamesPage/ModalJoin";
import { useAuth } from "../../contexts/AuthContext";
import { useNotification } from "../../contexts/NotificationContext";

const RoomsList = () => {
  const [rooms, setRooms] = useState([]);
  const [showJoinModal, setShowJoinModal] = useState(false);
  const { gameId } = useParams();
  const { getToken } = useAuth();
  const { addNotification } = useNotification();

  useEffect(() => {
    document.title = "Rooms List Page";

    const fetchRooms = async () => {
      try {
        const token = getToken(); 
        if (!token) {
          console.error("No token found!");
          return;
        }

        const response = await axios.get(
          `https://localhost:7005/api/services/rooms-service/rest/GetPublicRoomsList/${gameId}`,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );

        setRooms(response.data);
      } catch (err) {
        addNotification("Error fetching rooms", "error");
      }
    };

    fetchRooms();
  }, [gameId]);

  const handleCloseJoinModal = () => {
    setShowJoinModal(false);
  };

  return (
    <div className="rooms-container">
      <div className="rooms-header">
        <h3>Available Rooms for {gameId}</h3>
      </div>
      <table className="rooms-table">
        <thead>
          <tr>
            <th>Group Name</th>
            <th>Players</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          {rooms.length > 0 ? (
            rooms.map((room) => (
              <tr key={room.roomId}>
                <td>{room.roomName}</td>
                <td>{room.members.length}/{room.selectedPlayersCount}</td>
                <td>
                  <NavLink className="join-button" to={`/join/${room.roomId}`}>
                    Join
                  </NavLink>
                </td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="3">No rooms available for this game.</td>
            </tr>
          )}
        </tbody>
      </table>
      <ModalJoin showModal={showJoinModal} onClose={handleCloseJoinModal} />
    </div>
  );
};

export default RoomsList;
