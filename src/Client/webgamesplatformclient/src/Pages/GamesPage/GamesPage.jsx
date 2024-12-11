import { useEffect } from 'react';
import "./GamesPage.css"
const GameCard = ({ game }) => {
  return (
    <div className="game-card">
      <img alt={game.name} />
      <h3>{game.name}</h3>
    </div>
  );
};

const GamesContainer = ({ games }) => {
  return (
    <div className="games-container">
      {games.map((game, index) => (
        <GameCard key={index} game={game} />
      ))}
    </div>
  );
};

const GamesPage = () => {
  useEffect(() => {
    document.title = "Games";
}, []);
  const games = [
    { image: "Tic", name: 'Tic Tac Toe' },
    { image: "Chess", name: 'Chess' },
    { image: "Cube", name: 'Yahtzee' },
    { image: "Cube", name: 'Yahtzee' },
  ];

  return (
    <div className="page">
      <GamesContainer games={games} />
    </div>
  );
};

export default GamesPage;