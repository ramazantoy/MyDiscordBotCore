namespace DiscordBot.Entities
{
    public class Guild
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public Settings Settings { get; set; } = null!;
        public ulong? SettingsChannelId { get; set; }
    }
}