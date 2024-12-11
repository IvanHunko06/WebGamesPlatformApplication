import { useEffect } from 'react';
import "./LeaderPage.css"
const LeaderPage = () => {
  
  const leaderboardData = [
    { name: 'Denys', game: 'Tic-tac-toe', points: '1000 pts' },
    { name: 'Ivan', game: 'Chess', points: '990 pts' },
    { name: 'Vera', game: 'Sea battle', points: '880 pts' },
    { name: 'Dmytro', game: 'Tic-tac-toe', points: '636 pts' },
    { name: 'Danil', game: 'Chess', points: '500 pts' },
  ];
  useEffect(() => {
    document.title = "Leaderboard";
}, []);

  return (
    <div className="leaderboard-container">
      <div className="leaderboard-header">
        <h3>Season 2</h3>
        <p>November</p>
      </div>
      <table className="leaderboard-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Game</th>
            <th>Points</th>
          </tr>
        </thead>
        <tbody>
          {leaderboardData.map((player, index) => (
            <tr key={index}>
              <td>{player.name}</td>
              <td>{player.game}</td>
              <td>{player.points}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default LeaderPage;