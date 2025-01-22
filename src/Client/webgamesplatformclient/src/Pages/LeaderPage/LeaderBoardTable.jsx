import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import "./LeaderPage.css";

const LeaderBoardTable = ({ activeSeason, leaderboardData, seasonDates }) => {
  const [isTransitioning, setIsTransitioning] = useState(false);

  useEffect(() => {
    console.log(activeSeason, leaderboardData, seasonDates);
    setIsTransitioning(true);

    const timeout = setTimeout(() => {
      setIsTransitioning(false);
    }, 500);

    return () => clearTimeout(timeout);
  }, [activeSeason, leaderboardData, seasonDates]);

  return (
    <div
      className={`leaderboard-table-container ${
        isTransitioning ? "hidden" : ""
      }`}
    >
      <div className="leaderboard-header">
        <h3>Season {activeSeason}</h3>
        <p>
          {seasonDates.beginDate} / {seasonDates.endDate}
        </p>
      </div>
      <table className="leaderboard-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Points</th>
          </tr>
        </thead>
        <tbody>
          {leaderboardData.map((player, index) => (
            <tr key={index}>
              <td>{player.name}</td>
              <td>{player.points}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

LeaderBoardTable.propTypes = {
  activeSeason: PropTypes.number.isRequired,
  leaderboardData: PropTypes.arrayOf(
    PropTypes.shape({
      name: PropTypes.string.isRequired,
      points: PropTypes.string.isRequired,
    })
  ).isRequired,
  seasonDates: PropTypes.shape({
    beginDate: PropTypes.string.isRequired,
    endDate: PropTypes.string.isRequired,
  }).isRequired,
};

export default LeaderBoardTable;
