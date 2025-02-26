import React, { useState, useEffect } from "react";
import connection, { startSignalR } from "../Services/signalrService";
import {
  createGame,
  joinGame,
  sendGuess,
  lockInGuess,
  selectWord,
} from "../api/gameApi";

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

  const handleLockIn = async () => {
    if (!gameId) return;
    setLockedIn(!lockedIn);
    await lockInGuess(playerId, !lockedIn);
    connection.invoke("LockInGuess", gameId.toString(), playerId, !lockedIn);
  };

  const handleSelectWord = async (word: "Sponge" | "Bob") => {
    if (!gameId || role !== "host") return;
    setSelectedWord(word);
    await selectWord(gameId, word);
    connection.invoke("HostSelectWord", gameId.toString(), word);
  };

  return (
    <div className="game-board">
      <h2>Sponge or Bob?</h2>
      {!gameId ? (
        <>
          <button onClick={handleCreateGame}>Create Game</button>
          <input
            type="number"
            placeholder="Enter Game ID"
            value={tempGameId}
            onChange={(e) => setTempGameId(e.target.value)}
          />
          <button onClick={handleJoinGame}>Join Game</button>{" "}
        </>
      ) : (
        <>
          <h3>Game ID: {gameId}</h3>
          <h4>Role: {role}</h4>
          {role === "host" ? (
            <>
              <p>Select the correct word:</p>
              <button onClick={() => handleSelectWord("Sponge")}>Sponge</button>
              <button onClick={() => handleSelectWord("Bob")}>Bob</button>
            </>
          ) : (
            <>
              <p>Make your guess:</p>
              <button onClick={() => handleGuess("Sponge")}>Sponge</button>
              <button onClick={() => handleGuess("Bob")}>Bob</button>
            </>
          )}
          <button onClick={handleLockIn}>
            {lockedIn ? "Unlock" : "Lock In"}
          </button>
          <p>
            {selectedWord
              ? `The correct word is: ${selectedWord}`
              : "Waiting for host..."}
          </p>
        </>
      )}
    </div>
  );
};

export default GameBoard;
