import React, { useState } from "react";
import "./ProfilePage.css";
import { NavLink } from "react-router-dom";

const ProfileView = ({ 
  profileData, 
  setEditableName, 
  editableName, 
  setEditableDob, 
  editableDob, 
  isOwnProfile, 
  onSaveName, 
  onSaveDob, 
  onTogglePrivacy,
  icons,
  onIconSelect
}) => {
  const [isDropdownOpen, setDropdownOpen] = useState(false);
  const [selectedIcon, setSelectedIcon] = useState(profileData.smallImageUrl || "https://localhost:7005/resources/images/no-image-profile.png");

  const handleIconSelect = (iconId) => {
    console.log('first')
    onIconSelect(iconId);
    setSelectedIcon(icons.find(icon => icon.iconId === iconId)?.smallImageUrl || selectedIcon);
    setDropdownOpen(false);
  };

  return (
    <>
      <main>
        <div className="profile-container">
          <h1>{isOwnProfile ? "Your Profile" : `${profileData.publicName}'s Profile`}</h1>

          <div className="profile-details">
            <div className="photo">
              <img className="icon" src={selectedIcon} alt="None" />
              {isOwnProfile && (
                <div className="photo-dropdown">
                  <button className="change-photo" onClick={() => setDropdownOpen(!isDropdownOpen)}>
                    Change
                  </button>
                  {isDropdownOpen && (
                    <div className="dropdown-menu">
                      {icons.map((icon) => (
                        <img 
                          key={icon.iconId} 
                          src={icon.smallImageUrl} 
                          alt="Icon" 
                          className="dropdown-icon"
                          onClick={() => handleIconSelect(icon.iconId)}
                        />
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>

            <div className="profile-info">
              <div className="name_user">
                <label htmlFor="username">Player Name:</label>
                <input
                  type="text"
                  id="username"
                  placeholder="Enter your name"
                  value={editableName}
                  onChange={(e) => setEditableName(e.target.value)}
                  disabled={!isOwnProfile}
                />
                {isOwnProfile && (
                  <button className="save-btn" onClick={onSaveName}>
                    Save
                  </button>
                )}
              </div>

              {!isOwnProfile && !profileData.isPrivateProfile && (
                <>
                  <div className="date">
                    <label htmlFor="dob">Date of Birth:</label>
                    <input
                      type="date"
                      id="dob"
                      value={profileData.dob || ""}
                      disabled
                    />
                  </div>
                  <p className="total">Total: {profileData.score ?? "0"}</p>
                </>
              )}

              {isOwnProfile && (
                <>
                  <div className="date">
                    <label htmlFor="dob">Date of Birth:</label>
                    <input
                      type="date"
                      id="dob"
                      value={editableDob}
                      onChange={(e) => setEditableDob(e.target.value)}
                    />
                    <button className="save-btn" onClick={onSaveDob}>
                      Save
                    </button>
                  </div>
                  <p className="total">Total: {profileData.score ?? "0"}</p>
                  <div className="toggle-container">
                    <div
                      className={`toggle ${profileData.isPrivateProfile ? "private" : "public"}`}
                      onClick={onTogglePrivacy}
                    >
                      <div className="toggle-circle"></div>
                    </div>
                    <span className="toggle-text-profile">
                      {profileData.isPrivateProfile ? "Private" : "Public"}
                    </span>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      </main>

      {isOwnProfile && (
        <NavLink className="match-history-btn" to={`/match-history/${profileData.username}`}>
          Match History
        </NavLink>
      )}
    </>
  );
};

export default ProfileView;
