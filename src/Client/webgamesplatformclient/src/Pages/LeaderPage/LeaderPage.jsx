import React, { useState, useEffect } from "react";
import axios from "axios";
import { useAuth } from "../../contexts/AuthContext";
import LeaderBoardTable from "./LeaderBoardTable";
import "./LeaderPage.css";

const LeaderPage = () => {
  const { getToken } = useAuth();
  const [seasons, setSeasons] = useState([]);
  const [activeSeason, setActiveSeason] = useState(null);
  const [leaderboardData, setLeaderboardData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [fadeOut, setFadeOut] = useState(null);

  useEffect(() => {
    document.title = "Leader Page";
  }, []);

  useEffect(() => {
    const fetchSeasons = async () => {
      try {
        const token = await getToken();
        const response = await axios.get(
          "https://localhost:7005/api/services/rating-service/rest/GetSeasonsList/",
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        const data = response.data;
        setSeasons(data.slice().reverse());
        if (data.length > 0) {
          setActiveSeason(data[0].seasonId);
        }
      } catch (error) {
        console.error("Error fetching seasons:", error);
      }
    };

    fetchSeasons();
  }, [getToken]);

  useEffect(() => {
    if (activeSeason === null) return;

    setTimeout(() => {
      const fetchRatings = async () => {
        try {
          const token = await getToken();
          const response = await axios.get(
            `https://localhost:7005/api/services/rating-service/rest/GetRatingList/${activeSeason}`,
            {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );
          const data = response.data;
          const formattedData = data.map((item) => ({
            name: item.userId,
            points: `${item.score} pts`,
          }));
          setTimeout(() => {
            setLeaderboardData(formattedData);
            setFadeOut(false);
          }, 200);
        } catch (error) {
          console.error("Error fetching ratings:", error);
        } finally {
          setLoading(false);
        }
      };

      fetchRatings();
    }, 200);
  }, [activeSeason, getToken]);

  const handleSeasonChange = (seasonId) => {
    if (seasonId === activeSeason) return;
    setFadeOut(true);
    setTimeout(() => setActiveSeason(seasonId), 200);
  };

  const activeSeasonDates = seasons.find(
    (season) => season.seasonId === activeSeason
  );

  return (
    <div className="leaderboard-container">
      <div
        className={`season-buttons ${seasons.length > 10 ? "grid-container" : "flex-container"}`}
      >
        {seasons.map((season) => (
          <button
            key={season.seasonId}
            className={`season-button ${
              season.seasonId === activeSeason ? "active" : "inactive"
            }`}
            onClick={() => handleSeasonChange(season.seasonId)}
          >
            {season.seasonId}
          </button>
        ))}
      </div>
      {loading ? (
        <div className="loading">Loading...</div>
      ) : (
        <div className={`leaderboard-table-container ${fadeOut ? "fade-out" : "fade-in"}`}>
          <LeaderBoardTable
            activeSeason={activeSeason}
            leaderboardData={leaderboardData}
            seasonDates={
              activeSeasonDates
                ? { beginDate: activeSeasonDates.beginDate, endDate: activeSeasonDates.endDate }
                : { beginDate: "", endDate: "" }
            }
          />
        </div>
      )}
    </div>
  );
};

export default LeaderPage;
