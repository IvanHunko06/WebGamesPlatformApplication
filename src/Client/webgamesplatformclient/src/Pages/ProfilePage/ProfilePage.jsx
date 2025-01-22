import React, { useState, useEffect } from "react";
import "./ProfilePage.css";
import { useParams } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { useJwt } from "../../contexts/JwtTokenContext";
import axios from "axios";
import ProfileView from "./ProfileView";

const Profile = () => {
  const [profileData, setProfileData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const { username } = useParams();
  const { getToken } = useAuth();
  const { getUsername } = useJwt();

  useEffect(() => {
    document.title = "Profile";
  }, []);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const token = await getToken();
        const response = await axios.get(
          `https://localhost:7005/api/services/profile-service/rest/GetProfile/${username}`,
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
  }, [username, getToken]);

  const handleSaveName = () => {
    alert(`Name saved: ${profileData.publicName}`);
  };

  const handleSaveDob = () => {
    alert(`Date of Birth saved: ${profileData.dob}`);
  };

  const handleToggle = async () => {
    const newPrivacyStatus = !profileData.isPrivateProfile;

    try {
      const token = await getToken();
      await axios.patch(
        `https://localhost:7005/api/services/profile-service/rest/profile/${username}/privacy`,
        { isPrivateProfile: newPrivacyStatus },
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      setProfileData((prev) => ({
        ...prev,
        isPrivateProfile: newPrivacyStatus,
      }));
    } catch (err) {
      console.error("Error updating profile privacy:", err);
      alert("Failed to update profile privacy. Please try again.");
    }
  };

  const isOwnProfile = getUsername() === username;

  if (loading) return <p className="loading">Loading profile...</p>;
  if (error) return <p>{error}</p>;

  return (
    <ProfileView
      profileData={profileData}
      isOwnProfile={isOwnProfile}
      onSaveName={handleSaveName}
      onSaveDob={handleSaveDob}
      onTogglePrivacy={handleToggle}
    />
  );
};

export default Profile;