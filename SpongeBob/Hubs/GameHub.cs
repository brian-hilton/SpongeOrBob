using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SpongeBob.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinGame(string gameId, string playerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            await Clients.Group(gameId).SendAsync("PlayerJoined", playerId);
        }

        public async Task SendGuess(string gameId, string playerId, string guess)
        {
            await Clients.Group(gameId).SendAsync("ReceiveGuess", playerId, guess);
        }

        public async Task LockInGuess(string gameId, string playerId, bool LockedIn)
        {
            await Clients.Group(gameId).SendAsync("ReceiveLockIn", playerId, LockedIn);
        }

        public async Task HostSelectWord(string gameId, string word)
        {
            await Clients.Group(gameId).SendAsync("RevealAnswer", word);
        }
    }
}
