import React, { useState, useEffect } from "react";
import { NavLink } from "react-router-dom";

const ModalJoin = ({ showModal, onClose }) => {
  const [inputValue, setInputValue] = useState("");
  const [accessToken, setAccessToken] = useState("");
  const [isInputValid, setIsInputValid] = useState(true);
  const [isAccessTokenValid, setIsAccessTokenValid] = useState(true);

  const handleModalClick = (e) => {
    e.stopPropagation();
  };

  const handleInputChange = (e) => {
    setInputValue(e.target.value);
    setIsInputValid(true);
  };

  const handleAccessTokenChange = (e) => {
    setAccessToken(e.target.value);
    setIsAccessTokenValid(true);
  };

  useEffect(() => {
    if (showModal) {
      setInputValue("");
      setAccessToken("");
      setIsInputValid(true);
      setIsAccessTokenValid(true);
    }
  }, [showModal]);

  const validateInput = () => {
    if (!inputValue.trim()) {
      setIsInputValid(false); 
      return false;
    }
    return true;
  };

  if (!showModal) return null; 

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={handleModalClick}>
        <button className="modal-close" onClick={onClose}>
          &times;
        </button>
        <h2>Join</h2>
        <input
          type="text"
          placeholder="Enter code"
          className={`modal-input ${!isInputValid ? "invalid" : ""}`}
          value={inputValue}
          onChange={handleInputChange}
        />
        <input
          type="text"
          placeholder="Enter access token"
          className={`modal-input ${!isAccessTokenValid ? "invalid" : ""}`}
          value={accessToken}
          onChange={handleAccessTokenChange}
        />
        <NavLink
          to={`/join/${inputValue}?accessToken=${accessToken}`}
          className={`modal-continue ${!(isInputValid && isAccessTokenValid) ? "disabled-link" : ""}`}
          onClick={(e) => {
            if (!validateInput() ) {
              e.preventDefault(); 
            } else {
              onClose();
            }
          }}
        >
          Continue
        </NavLink>
      </div>
    </div>
  );
};

export default ModalJoin;
