import axios from "axios";

const API_URL = "http://localhost:5258/api/game"; // Replace with your backend URL

export const createGame = async (hostPlayerId: string) => {
  const response = await axios.post(
    `${API_URL}/create`,
    JSON.stringify({ hostPlayerId }), // Convert to JSON
    {
      headers: { "Content-Type": "application/json" },
    }
  );
  return response.data;
};

export const joinGame = async (gameId: number, playerId: string) => {
  const response = await axios.post(
    `${API_URL}/join/${gameId}/${playerId}`,
    null, 
    {
      headers: { "Content-Type": "application/json" },
    }
  );
  return response.data;
};

export const sendGuess = async (gameId: number, playerId: string, guess: string) => {
  const response = await axios.post(
    `${API_URL}/player/guess/${gameId}/${playerId}/${guess}`,
    null,
    {
      headers: { "Content-Type": "application/json" },
    }
  );
  return response.data;
};

export const lockInGuess = async (playerId: string, isLocked: boolean) => {
  const response = await axios.post(
    `${API_URL}/player/lockin/${playerId}/${isLocked}`,
    JSON.stringify({ isLocked }), 
    {
      headers: { "Content-Type": "application/json" },
    }
  );
  return response.data;
};

export const selectWord = async (gameId: number, word: string) => {
  const response = await axios.post(
    `${API_URL}/host/select/${gameId}/${word}`,
    JSON.stringify({ word }), 
    {
      headers: { "Content-Type": "application/json" },
    }
  );
  return response.data;
};
