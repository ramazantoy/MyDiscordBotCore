using DiscordBot.Entities;

namespace DiscordBot.Interfaces;

public interface ISettingsRepository : IRepository<Settings>
{
    Task<Settings> GetSettingsByGuildIdAsync(int guildId);
    
    
}