import React, { useState, useEffect } from "react";
import { useParams, NavLink } from "react-router-dom";
import "./RoomsListPage.css";
import ModalJoin from "../GamesPage/ModalJoin";

const RoomsList = () => {
  const [showJoinModal, setShowJoinModal] = useState(false);
  const { gameId } = useParams();

  const roomsData = [
    { id: 1, name: "Group Alpha", players: "1/2" },
    { id: "a38c0c42-b1d6-47b9-9185-2d367da1dbdf", name: "Group Beta", players: "1/2"},
    { id: 3, name: "Group Gamma", players: "0/3" },
    { id: 4, name: "Group Delta", players: "1/4"},
  ];

  const filteredRooms = roomsData.filter((room) => room.gameId === gameId);

  useEffect(() => {
    document.title = "RoomListPage";
  }, []);


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
          {filteredRooms.length > 0 ? (
            filteredRooms.map((room) => (
              <tr key={room.id}>
                <td>{room.name}</td>
                <td>{room.players}</td>
                <td>
                  <NavLink className="join-button" to={`/join/${room.id}`}>
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
