namespace DiscordBot.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int? GuildId { get; set; }
        public Guild? Guild { get; set; }
    }
}