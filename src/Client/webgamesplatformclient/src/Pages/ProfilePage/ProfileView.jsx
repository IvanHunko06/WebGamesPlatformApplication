import React from "react";
import "./ProfilePage.css";
import { NavLink } from "react-router-dom";

const ProfileView = ({ profileData, isOwnProfile, onSaveName, onSaveDob, onTogglePrivacy }) => {
  return (
    <>
      <main>
        <div className="profile-container">
          <h1>{isOwnProfile ? "Your Profile" : `${profileData.publicName}'s Profile`}</h1>

          <div className="profile-details">
            <div className="photo">
              <img
                src={profileData.smallImageUrl || "src/assets/profiel_ico.png"}
                alt="Profile Picture"
              />
              {isOwnProfile && <button className="change-photo">Change</button>}
            </div>

            <div className="profile-info">
              <div className="name_user">
                <label htmlFor="username">Player Name:</label>
                <input
                  type="text"
                  id="username"
                  placeholder="Enter your name"
                  value={profileData.publicName || ""}
                  onChange={(e) =>
                    profileData.setPublicName(e.target.value)
                  }
                  disabled={!isOwnProfile}
                />
                {isOwnProfile && (
                  <button className="save-btn" onClick={onSaveName}>
                    Save
                  </button>
                )}
              </div>

              {!isOwnProfile && !profileData.isPrivate && (
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
                  <p className="total">Total: 1000</p>
                </>
              )}

              {isOwnProfile && (
                <>
                  <div className="date">
                    <label htmlFor="dob">Date of Birth:</label>
                    <input
                      type="date"
                      id="dob"
                      value={profileData.dob || ""}
                      onChange={(e) =>
                        profileData.setDob(e.target.value)
                      }
                    />
                    <button className="save-btn" onClick={onSaveDob}>
                      Save
                    </button>
                  </div>
                  <p className="total">Total: 1000</p>
                  <div className="toggle-container">
                    <div
                      className={`toggle ${
                        profileData.isPrivateProfile ? "private" : "public"
                      }`}
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
      {/* user cannot  */}
      {isOwnProfile && (  
        <NavLink className="match-history-btn" to={`/match-history/${profileData.username}`}>
          Match History
        </NavLink>
      )}
    </>
  );
};

export default ProfileView;
