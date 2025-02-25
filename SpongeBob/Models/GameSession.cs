using System.ComponentModel.DataAnnotations;

namespace SpongeBob.Models
{
    public class GameSession
    {
        [Key]
        public int Id { get; set; }
        public string HostPlayerId { get; set; } = string.Empty;
        public string? GuesserPlayerId { get; set; }
        public string? SelectedWord { get; set; } // sponge or bob
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}
