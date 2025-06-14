import React, { useState, useEffect } from "react";
import "./ProfilePage.css";
import { useParams } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { useJwt } from "../../contexts/JwtTokenContext";
import axios from "axios";
import ProfileView from "./ProfileView";
import { useNotification } from "../../contexts/NotificationContext";

const Profile = () => {
  const [profileData, setProfileData] = useState(null);
  const [editableName, setEditableName] = useState("");
  const [editableDob, setEditableDob] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [icons, setIcons] = useState([]);
  const { username } = useParams();
  const { getToken } = useAuth();
  const { getUsername } = useJwt();
  const { addNotification } = useNotification();

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
        setEditableName(response.data.publicName || "");
        setEditableDob(response.data.dob || "");
        setLoading(false);
      } catch (err) {
        addNotification("Error fetching profile data","error");
        setError("Failed to fetch profile data.");
        setLoading(false);
      }
    };

    fetchProfile();
  }, [username, getToken]);

  const handleSaveProfileData = async () => {
    try {
      const token = await getToken();
      const payload = {
        dateOfBirthday: editableDob,
        publicName: editableName,
      };

      await axios.put(
        `https://localhost:7005/api/services/profile-service/rest/profile/${username}/update`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      setProfileData((prev) => ({
        ...prev,
        publicName: editableName,
        dob: editableDob,
      }));
      addNotification("Profile updated");

    } catch (err) {
      addNotification("Error updating profile data", "error");
    }
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
      addNotification("Privacy updated");
    } catch (err) {
      addNotification("Error updating profile privacy", "error");
    }
  };

  useEffect(() => {
    const fetchScore = async () => {
      try {
        const token = await getToken();
        const response = await axios.get(
          `https://localhost:7005/api/services/rating-service/rest/GetScore/${username}`,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        setProfileData((prev) => ({
          ...prev,
          score: response.data, 
        }));
      } catch (err) {
        // addNotification("Error fetching score data","error");
        
      }
    };
  
    if (username) fetchScore();
  }, [username, getToken]);

  useEffect(() => {
    const fetchIcons = async () => {
      try {
        const token = await getToken();
        const response = await axios.get(
          "https://localhost:7005/api/services/profile-service/rest/GetProfileIcons",
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        console.log(response.data)
        setIcons(response.data);
      } catch (err) {
        addNotification("Error fetching profile icons","error");
      }
    };

    fetchIcons();
  }, [getToken]);

  const handleSetUserIcon = async (iconId) => {
    try {
      const token = await getToken();
      await axios.patch(
        `https://localhost:7005/api/services/profile-service/rest/profile/${username}/icon`,
        { IconId: iconId },
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
  
      setProfileData((prev) => ({
        ...prev,
        smallImageUrl: icons.find((icon) => icon.iconId === iconId)?.smallImageUrl || prev.smallImageUrl,
      }));
      addNotification("Icon updated");
    } catch (err) {
      addNotification("Error updating profile icon", "error");
    }
  };

  const isOwnProfile = getUsername() === username;

  if (loading) return <p className="loading">Loading profile...</p>;
  if (error) return <p>{error}</p>;

  return (
    <ProfileView
      profileData={profileData}
      editableName={editableName}
      setEditableName={setEditableName}
      editableDob={editableDob}
      setEditableDob={setEditableDob}
      isOwnProfile={isOwnProfile}
      onSaveName={handleSaveProfileData}
      onSaveDob={handleSaveProfileData}
      onTogglePrivacy={handleToggle}
      icons={icons}
      onIconSelect={handleSetUserIcon}
    />
  );
};

export default Profile;
