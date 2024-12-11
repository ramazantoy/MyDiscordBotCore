using DiscordBot.Entities;

namespace DiscordBot.Interfaces;

public interface ISettingsRepository : IRepository<Settings>
{
    Task<Settings> GetByGuildIdAsync(int guildId);
    
    
}