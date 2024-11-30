import React, { useState } from 'react';

const Profile = () => {
  const [username, setUsername] = useState('');
  const [dob, setDob] = useState('');
  const [total, setTotal] = useState(1000); 

  const handleSaveName = () => {
    alert(`Name saved: ${username}`);
  };

  const handleSaveDob = () => {
    alert(`Date of Birth saved: ${dob}`);
  };

  return (
    <main>
      <div className="profile-container">
        <h1>Your Profile</h1>

        <div className="profile-details">
          <div className="photo">
            <img src="src\assets\profiel_ico.png" alt="Profile Picture" />
            <button className="change-photo">Change</button>
          </div>

          <div className="profile-info">
            <div className="name">
              <label htmlFor="username">Player Name:</label>
              <input
                type="text"
                id="username"
                placeholder="Enter your name"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
              <button className="save-btn" onClick={handleSaveName}>
                Save
              </button>
            </div>

            <div className="date">
              <label htmlFor="dob">Date of Birth:</label>
              <input
                type="date"
                id="dob"
                value={dob}
                onChange={(e) => setDob(e.target.value)}
              />
              <button className="save-btn" onClick={handleSaveDob}>
                Save
              </button>
            </div>

            <p className="total">Total: {total}</p>
          </div>
        </div>
      </div>
    </main>
  );
};

export default Profile;
