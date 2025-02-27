import React, { useState, useEffect } from "react";
import connection, { startSignalR } from "../Services/signalrService";
import {
  createGame,
  joinGame,
  sendGuess,
  selectWord,
  handleLockIn,
} from "../api/gameApi";
import "../styles/GameBoard.css";

const GameBoard = () => {
  const [gameId, setGameId] = useState<number | null>(null);
  const [tempGameId, setTempGameId] = useState<string>("");
  const [playerId, setPlayerId] = useState<string>(
    `player-${Math.random().toString(36).substr(2, 5)}`
  );
  const [role, setRole] = useState<"host" | "guesser" | null>(null);
  const [guess, setGuess] = useState<string | null>(null);
  const [lockedIn, setLockedIn] = useState<boolean>(false);
  const [selectedWord, setSelectedWord] = useState<string | null>(null);

  useEffect(() => {
    startSignalR();
  }, []);

  const handleCreateGame = async () => {
    const game = await createGame(playerId);
    setGameId(game.id);
    setRole("host");
    connection.invoke("JoinGame", game.id.toString(), playerId);
  };

  const handleJoinGame = async () => {
    const id = Number(tempGameId);
    if (isNaN(id)) {
      alert("Please enter a valid game ID.");
      return;
    }
    await joinGame(id, playerId);
    setGameId(id);
    setRole("guesser");
    connection.invoke("JoinGame", id.toString(), playerId);
  };

  const handleGuess = async (word: "Sponge" | "Bob") => {
    if (!gameId) return;
    setGuess(word);
    await sendGuess(gameId, playerId, word);
    connection.invoke("SendGuess", gameId.toString(), playerId, word);
  };

  return (
    <div className="game-container">
      <header className="game-header">
        <h1>Sponge or Bob?</h1>
        {gameId && <p className="game-id">Game ID: {gameId}</p>}
      </header>

      <div className="game-main">
        {!gameId ? (
          <div className="game-setup">
            <button onClick={handleCreateGame}>Create Game</button>
            <input
              type="number"
              placeholder="Enter Game ID"
              value={tempGameId}
              onChange={(e) => setTempGameId(e.target.value)}
            />
            <button onClick={handleJoinGame}>Join Game</button>
          </div>
        ) : (
          <div className="game-board">
            <h3>Role: {role}</h3>
            <p className="lock-status">Locked In: {lockedIn ? "Yes" : "No"}</p>

            {role === "host" ? (
              <div className="selection-area">
                <p>Select the correct word:</p>
                <button
                  className={selectedWord === "Sponge" ? "selected" : ""}
                  onClick={() => setSelectedWord("Sponge")}
                >
                  Sponge
                </button>
                <button
                  className={selectedWord === "Bob" ? "selected" : ""}
                  onClick={() => setSelectedWord("Bob")}
                >
                  Bob
                </button>
              </div>
            ) : (
              <div className="selection-area">
                <p>Make your guess:</p>
                <button
                  className={guess === "Sponge" ? "selected" : ""}
                  onClick={() => handleGuess("Sponge")}
                >
                  Sponge
                </button>
                <button
                  className={guess === "Bob" ? "selected" : ""}
                  onClick={() => handleGuess("Bob")}
                >
                  Bob
                </button>
              </div>
            )}

            <div className="lock-buttons">
              <button
                className={lockedIn ? "selected" : ""}
                onClick={() => handleLockIn(playerId, true)}
              >
                Lock In
              </button>
              <button
                className={!lockedIn ? "selected" : ""}
                onClick={() => handleLockIn(playerId, false)}
              >
                Unlock
              </button>
            </div>

            <p>
              {selectedWord
                ? `Choose your winning word: ${selectedWord}`
                : "Waiting for Sponge Man to make up his mind..."}
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default GameBoard;
