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
    const fetchRatings = async () => {
      if (activeSeason === null) return;

      setLoading(true);
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
        console.log(response.data);
        const formattedData = data.map((item) => ({
          name: item.userId,
          points: `${item.score} pts`,
        }));
        setLeaderboardData(formattedData);
      } catch (error) {
        console.error("Error fetching ratings:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchRatings();
  }, [activeSeason, getToken]);

  const handleSeasonChange = (seasonId) => {
    if (seasonId === activeSeason) return;
    setActiveSeason(seasonId);
  };

  const activeSeasonDates = seasons.find(
    (season) => season.seasonId === activeSeason
  );

  return (
    <div className="leaderboard-container">
      <div className="season-buttons">
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
        <LeaderBoardTable
          activeSeason={activeSeason}
          leaderboardData={leaderboardData}
          seasonDates={
            activeSeasonDates
              ? { beginDate: activeSeasonDates.beginDate, endDate: activeSeasonDates.endDate }
              : { beginDate: "", endDate: "" }
          }
        />
      )}
    </div>
  );
};

export default LeaderPage;
