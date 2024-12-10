namespace DiscordBot.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int GuildId { get; set; }
        public Guild Guild { get; set; } = null!;
    }
}