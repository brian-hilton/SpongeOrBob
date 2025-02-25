using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpongeBob.Models;

namespace SpongeBob.Controllers
{
    [Route("api/game")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly GameDbContext _context;
        public GameController(GameDbContext context)
        {
            _context = context;
        }

        // Create game session
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] string hostPlayerId)
        {
            var game = new GameSession
            {
                HostPlayerId = hostPlayerId,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(5)
            };

            _context.GameSessions.Add(game);
            await _context.SaveChangesAsync();
            return Ok(game);
        }

        // Join game instance, verify game and playerId
        [HttpPost("join/{gameId}/{playerId}")]
        public async Task<IActionResult> JoinGame(int gameId, string playerId)
        {
            var game = await _context.GameSessions.FindAsync(gameId);
            if (game == null || game.GuesserPlayerId != null) return NotFound("Game not found or full.");

            game.GuesserPlayerId = playerId;
            await _context.SaveChangesAsync();
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
            return Ok(game);
        }

        // Lock in
        [HttpPost("player/lockin/{playerId}/{lockinState}")]
        public async Task<IActionResult> PlayerLockIn(string playerId, bool lockInState)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null) return NotFound("Player not found");

            player.LockedIn = lockInState;
            await _context.SaveChangesAsync();
            return Ok(player);
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
