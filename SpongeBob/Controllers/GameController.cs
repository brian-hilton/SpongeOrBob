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

        [HttpPost("join/{gameId}/{playerId}")]
        public async Task<IActionResult> JoinGame(int gameId, string playerId)
        {
            var game = await _context.GameSessions.FindAsync(gameId);
            if (game == null || game.GuesserPlayerId != null) return NotFound("Game not found or full.");

            game.GuesserPlayerId = playerId;
            await _context.SaveChangesAsync();
            return Ok(game);
        }

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
    }
}
