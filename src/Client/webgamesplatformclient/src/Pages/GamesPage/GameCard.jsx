import React, { useState } from "react";
import ModalCreate from "./ModalCreate";
import { NavLink } from "react-router-dom";

const GameCard = ({ game }) => {
  const [showButtons, setShowButtons] = useState(false);
  const [showSubButtons, setShowSubButtons] = useState(false);
  const [fadeOut, setFadeOut] = useState(false);
  const [showJoinModal, setShowJoinModal] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);

  const handleClick = (type) => {
    if (type === "rating") {
      setShowSubButtons(true);
    } else {
      setShowSubButtons(false);
    }
  };

  const handleMouseLeave = () => {
    setFadeOut(true);
    setTimeout(() => {
      setShowButtons(false);
      setShowSubButtons(false);
      setFadeOut(false);
    }, 300);
  };

  const handleCardClick = () => {
    if (showButtons) {
      setFadeOut(true);
      setTimeout(() => {
        setShowButtons(false);
        setShowSubButtons(false);
        setFadeOut(false);
      }, 300);
    } else {
      setShowButtons(true);
    }
  };

  const handleCreateModalClose = () => {
    setShowCreateModal(false);
  };

  const isExpanded = showButtons || showSubButtons;

  return (
    <div
      className={`game-card ${isExpanded ? "expanded" : "collapsed"}`}
      onClick={handleCardClick}
      onMouseLeave={handleMouseLeave}
    >
      <img src={game.image} alt={game.name} />
      <h3>{game.name}</h3>

      {showButtons && (
        <div
          className={`button-container ${fadeOut ? "fade-out" : ""}`}
          onClick={(e) => e.stopPropagation()}
        >
          {!showSubButtons ? (
            <>
              {/* <button onClick={() => handleClick("solo")} className="button">
                Solo
              </button> */}
              <button onClick={() => handleClick("rating")} className="button-rating">
                Rating
              </button>
            </>
          ) : (
            <div
              className={`sub-button-container ${fadeOut ? "fade-out" : ""}`}
            >
              <NavLink to={`/rooms-list/${game.id}`} className="button">
                Join
              </NavLink>
              <button
                onClick={() => setShowCreateModal(true)}
                className="button"
              >
                Create
              </button>
            </div>
          )}
        </div>
      )}

      <ModalCreate showModal={showCreateModal} onClose={handleCreateModalClose} game={game} />
    </div>
  );
};

export default GameCard;
