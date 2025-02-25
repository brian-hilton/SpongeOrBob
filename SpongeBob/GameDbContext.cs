using Microsoft.EntityFrameworkCore;
using SpongeBob.Models;

namespace SpongeBob
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<Player> Players { get; set; }
    }
}
