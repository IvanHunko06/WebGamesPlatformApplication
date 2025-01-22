import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import axios from "axios";
import "./MatchHistoryPage.css";

const MatchHistoryPage = () => {
  const { username } = useParams();
  const { getToken } = useAuth();
  const [matches, setMatches] = useState([]);
  const [games, setGames] = useState({});
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
      document.title = `${username}'s Match History`;
  }, [username]);

  useEffect(() => {
    const fetchMatches = async () => {
      try {
        const token = await getToken();
        const response = await axios.get(
          `https://localhost:7005/api/services/match-history-service/rest/GetMatchesInfo/${username}`,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        if (Array.isArray(response.data)) {
          setMatches(response.data);
          console.log(response.data);
        } else {
          console.error("Unexpected response format:", response.data);
        }
      } catch (error) {
        console.error("Error fetching matches:", error);
      }
    };

    const fetchGames = async () => {
      try {
        const token = await getToken();
        const response = await axios.get(
          'https://localhost:7005/api/services/games-service/rest/GetGamesList/',
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );

        const fetchedGames = response.data.reduce((acc, game) => {
          acc[game.gameId] = game.localizationKey;
          return acc;
        }, {});
        setGames(fetchedGames);
      } catch (error) {
        console.error("Failed to fetch games:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchMatches();
    fetchGames();
  }, [getToken, username]);

  const formatPoints = (points) => {
    return points > 0 ? `+${points}` : `${points}`;
  };

  const formatReason = (reason) => {
    return reason
      .toLowerCase()
      .replace(/_/g, " ")
      .replace(/^\w/, (c) => c.toUpperCase());
  };

  const getUserScore = (userScoreDelta) => {
    return userScoreDelta[username] ?? 0; // Use username as the key to get their specific score
  };

  return (
    <div className="match-history-container">
      <div className="match-history-header">
        <h3>{username ? `${username}'s Game Sessions` : "Game Sessions"}</h3>
      </div>
      <div className="match-history-table-container">
        <table className="match-history-table">
          <thead>
            <tr>
              <th style={{ width: "20%" }}>Game Type</th>
              <th>Start Time</th>
              <th>End Time</th>
              <th>Reason</th>
              <th>{username ? `${username}'s Points` : "Player Points"}</th>
            </tr>
          </thead>
          <tbody>
            {matches.length > 0 ? (
              matches.map((match) => (
                <tr key={match.timeBegin}>
                  <td>{games[match.gameId] || match.gameId}</td>
                  <td>{new Date(match.timeBegin).toLocaleString()}</td>
                  <td>{new Date(match.timeEnd).toLocaleString()}</td>
                  <td>{formatReason(match.finishReason)}</td>
                  <td>{formatPoints(match.gainedScore)}</td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan="5" style={{ textAlign: "center" }}>
                  No match history available.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default MatchHistoryPage;
