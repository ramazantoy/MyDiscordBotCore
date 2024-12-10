namespace DiscordBot.Entities
{
    public class User
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsBot { get; set; }
    }
}