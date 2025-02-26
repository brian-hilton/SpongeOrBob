using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SpongeBob.Hubs;
using SpongeBob.Models;

namespace SpongeBob.Controllers
{
    [Route("api/game")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly GameDbContext _context;
        private readonly IHubContext<GameHub> _hubContext;
        public GameController(GameDbContext context, IHubContext<GameHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Create game session
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.HostPlayerId))
            {
                return BadRequest("Invalid request: HostPlayerId is required.");
            }

            var game = new GameSession
            {
                HostPlayerId = request.HostPlayerId,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(5) // Game lasts 5 minutes
            };

            _context.GameSessions.Add(game);
            await _context.SaveChangesAsync();

            return Ok(game);
        }
        public class CreateGameRequest
        {
            public string HostPlayerId { get; set; }
        }

        // Join game instance, verify game and playerId
        [HttpPost("join/{gameId}/{playerId}")]
        public async Task<IActionResult> JoinGame(int gameId, string playerId)
        {
            Console.WriteLine($"[DEBUG] Player {playerId} attempting to join game {gameId}");

            var game = await _context.GameSessions.FindAsync(gameId);
            if (game == null || game.GuesserPlayerId != null)
            {
                return NotFound("Game not found or already full");
            }

            game.GuesserPlayerId = playerId;

            var existingPlayer = await _context.Players.FindAsync(playerId);
            if (existingPlayer == null)
            {
                Console.WriteLine($"[DEBUG] Player {playerId} not found, inserting new player.");

                var newPlayer = new Player
                {
                    Id = playerId,
                    Username = $"Player-{playerId.Substring(playerId.Length - 5)}",
                    CurrentGameSessionId = gameId
                };

                _context.Players.Add(newPlayer);
                Console.WriteLine($"[DEBUG] Adding Player {playerId} to EF Core context.");
            }
            else
            {
                Console.WriteLine($"[DEBUG] Player {playerId} already exists in database.");
            }

            int changes = await _context.SaveChangesAsync();
            Console.WriteLine($"[DEBUG] SaveChanges Result: {changes} rows affected");

            await _hubContext.Clients.Group(gameId.ToString()).SendAsync("PlayerJoined", playerId);

            return Ok(game);
        }




        // Choose word
        [HttpPost("host/select/{gameId}/{word}")]
        public async Task<IActionResult> HostSelectWord(int gameId, string word)
        {
            var game = await _context.GameSessions.FindAsync(gameId);
            if (game == null) return NotFound("Game not found.");

            if (word != "Sponge" && word != "Bob") return BadRequest("Must choose Sponge or Bob!");

            game.SelectedWord = word;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(gameId.ToString()).SendAsync("RevealAnswer", word);
            return Ok(game);
        }

        [HttpPost("player/guess/{gameId}/{playerId}/{guess}")]
        public async Task<IActionResult> PlayerGuess(int gameId, string playerId, string guess)
        {
            Console.WriteLine($"Received Guess Request: gameId={gameId}, playerId={playerId}, guess={guess}");
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                Console.WriteLine($"Player not found: {playerId}");
                return NotFound("Player not found");
            }

            if (guess != "Sponge" && guess != "Bob")
            {
                return BadRequest("Invalid guess.");
            }

            player.Guess = guess;
            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group(gameId.ToString()).SendAsync("ReceiveGuess", playerId, guess);
            return Ok(player);
        }


        // Lock in
        [HttpPost("player/lockin/{playerId}/{lockinState}")]
        public async Task<IActionResult> PlayerLockIn(string playerId, string lockinState)
        {
            Console.WriteLine($"[DEBUG] Lock-in request: playerId={playerId}, lockinState={lockinState}");

            bool isLocked;
            if (!bool.TryParse(lockinState, out isLocked))  
            {
                Console.WriteLine("[ERROR] Invalid boolean value");
                return BadRequest("Invalid boolean value");
            }

            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                Console.WriteLine($"[ERROR] Player {playerId} not found");
                return NotFound("Player not found");
            }

            player.LockedIn = isLocked;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(player.CurrentGameSessionId.ToString()).SendAsync("ReceiveLockIn", playerId, isLocked);

            return Ok(new { playerId, isLocked });
        }


        // Get game session
        [HttpGet("status/{gameId}")]
        public async Task<IActionResult> GetGameStatus(int gameId)
        {
            var game = await _context.GameSessions
                .Include(g => g.HostPlayerId)
                .Include(g => g.GuesserPlayerId)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null) return NotFound("Game not found");

            return Ok(game);
        }
    }
}
