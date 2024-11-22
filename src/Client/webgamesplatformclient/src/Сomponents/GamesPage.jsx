import React, { useEffect } from 'react';
import Tic from '../assets/Tic.jpg';
import Chess from '../assets/Chess.jpg';
import Cube from '../assets/Cube.jpg';
const GameCard = ({ game }) => {
  return (
    <div className="game-card">
      <img src={game.image} alt={game.name} />
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
    { image: Tic, name: 'Tic Tac Toe' },
    { image: Chess, name: 'Chess' },
    { image: Cube, name: 'Yahtzee' },
  ];

  return (
    <div className="page">
      <GamesContainer games={games} />
    </div>
  );
};

export default GamesPage;
