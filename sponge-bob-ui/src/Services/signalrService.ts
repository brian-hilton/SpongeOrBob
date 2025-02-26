import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
.withUrl("http://localhost:5258/gameHub")
.withAutomaticReconnect()
.configureLogging(signalR.LogLevel.Information)
.build();

export const startSignalR = async () => {
    if (connection.state === signalR.HubConnectionState.Connected) {
      console.log("SignalR is already connected.");
      return;
    }
  
    try {
      await connection.start();
      console.log("SignalR Connected");
    } catch (err) {
      console.error("SignalR Connection Error:", err);
    }
  };
  
  // Listening for real-time events
  connection.on("PlayerJoined", (playerId) => {
    console.log(`Player ${playerId} joined the game.`);
  });
  
  connection.on("ReceiveGuess", (playerId, guess) => {
    console.log(`Player ${playerId} guessed: ${guess}`);
  });
  
  connection.on("ReceiveLockIn", (playerId, isLocked) => {
    console.log(`Player ${playerId} lock-in state: ${isLocked}`);
  });
  
  connection.on("RevealAnswer", (word) => {
    console.log(`The correct word is: ${word}`);
  });
  
  export default connection;
