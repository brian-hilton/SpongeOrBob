using System.ComponentModel.DataAnnotations;

namespace SpongeBob.Models
{
    public class Player
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; } = "Anon";
        public int? CurrentGameSessionId { get; set; }
        public bool LockedIn { get; set; } = false;
        public string? Guess {  get; set; } // sponge or bob
    }
}
