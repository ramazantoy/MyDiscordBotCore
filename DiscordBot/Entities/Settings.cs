namespace DiscordBot.Entities
{
    public class Settings
    {
        public int Id { get; set; }
        public int GuildId { get; set; } = 0;
        public Guild? Guild { get; set; } = null!;
        public int SecurityLevel { get; set; } = 0;
        public bool WelcomeMessageEnabled { get; set; } = true;
        
        public ulong? SettingsChannelId { get; set; }
        public ulong? WelcomeChannelId { get; set; }
    }
}