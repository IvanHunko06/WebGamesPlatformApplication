import React, { useState, useEffect } from "react";
import "./ProfilePage.css";
import { NavLink } from "react-router-dom";
import { useJwt } from "../../contexts/JwtTokenContext";
import { useAuth } from "../../contexts/AuthContext";
import axios from "axios";

const Profile = () => {
  const [profileData, setProfileData] = useState(null); 
  const [loading, setLoading] = useState(true); 
  const [error, setError] = useState(null); 

  const { getUsername } = useJwt();
  const { getToken } = useAuth();

  useEffect(() => {
    document.title = "Profile";
  }, []);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const token = await getToken();
        const response = await axios.get(
          "https://localhost:7005/api/services/profile-service/rest/GetProfile/user",
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        setProfileData(response.data);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching profile data:", err);
        setError("Failed to fetch profile data.");
        setLoading(false);
      }
    };

    fetchProfile();
  }, [getToken]);

  const handleSaveName = () => {
    alert(`Name saved: ${profileData.publicName}`);
  };

  const handleSaveDob = () => {
    alert(`Date of Birth saved: ${profileData.dob}`);
  };

  const handleToggle = () => {
    setProfileData((prev) => ({
      ...prev,
      isPrivateProfile: !prev.isPrivateProfile,
    }));
  };

  if (loading) return <p className="loading">Loading profile...</p>;
  if (error) return <p>{error}</p>;

  return (
    <>
      <main>
        <div className="profile-container">
          <h1>Your Profile</h1>

          <div className="profile-details">
            <div className="photo">
              <img
                src={profileData.smallImageUrl || "src/assets/profiel_ico.png"}
                alt="Profile Picture"
              />
              <button className="change-photo">Change</button>
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
                    setProfileData({ ...profileData, publicName: e.target.value })
                  }
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
                  value={profileData.dob || ""}
                  onChange={(e) =>
                    setProfileData({ ...profileData, dob: e.target.value })
                  }
                />
                <button className="save-btn" onClick={handleSaveDob}>
                  Save
                </button>
              </div>

              <p className="total">Total: 1000</p>

              <div className="toggle-container">
                <div
                  className={`toggle ${profileData.isPrivateProfile ? "private" : "public"}`}
                  onClick={handleToggle}
                >
                  <div className="toggle-circle"></div>
                </div>
                <span className="toggle-text-profile">
                  {profileData.isPrivateProfile ? "Private" : "Public"}
                </span>
              </div>
            </div>
          </div>
        </div>
      </main>
      <NavLink className="match-history-btn" to={`/match-history/${getUsername()}`}>
        Match History
      </NavLink>
    </>
  );
};

export default Profile;
