import React, { useEffect, useState } from 'react';
import axios from 'axios';
import Tic from '../../assets/Tic.jpg';
import GameCard from './GameCard';
import './GamesPage.css';
import ModalJoin from './ModalJoin';
import { useAuth } from "../../contexts/AuthContext";

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
  const [games, setGames] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showJoinModal, setShowJoinModal] = useState(false);
  const { getToken } = useAuth(); 

  useEffect(() => {
    document.title = 'Games';

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
        
        const fetchedGames = response.data.map((game, index) => ({
          id: game.gameId,
          image: game.imageUrl, 
          name: game.localizationKey,
          minPlayers: game.minPlayersCount,
          maxPlayers: game.maxPlayersCount,
          singlePlayer: game.supportSinglePlayer,
        }));
        setGames(fetchedGames);

      } catch (error) {
        console.error('Failed to fetch games:', error);
      } finally {
        setIsLoading(false); 
      }
    };

    fetchGames();
  }, [getToken]);

  const handleOpenJoinModal = () => {
    setShowJoinModal(true);
  };

  const handleCloseJoinModal = () => {
    setShowJoinModal(false);
  };

  return (
    <div className="fullpage">
      {isLoading ? (
        <div className="loading">Loading games...</div>
      ) : (
        <>
          <button className="code-button" onClick={handleOpenJoinModal}>
            Connect using code
          </button>
          <div className="page">
            <GamesContainer games={games} />
          </div>
          <ModalJoin showModal={showJoinModal} onClose={handleCloseJoinModal} />
        </>
      )}
    </div>
  );
};

export default GamesPage;
