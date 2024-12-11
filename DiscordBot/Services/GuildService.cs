using DiscordBot.Entities;
using DiscordBot.Interfaces;

namespace DiscordBot.Services;

public class GuildService
{
    private readonly IGuildRepository _guildRepository;

    public GuildService(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }
    
    public async Task<Guild> GetGuildById(ulong guildId)
    {
        return await _guildRepository.GetByDcId(guildId);
    }
}