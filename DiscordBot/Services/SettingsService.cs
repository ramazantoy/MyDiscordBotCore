using DiscordBot.Entities;
using DiscordBot.Interfaces;
using DiscordBot.Repositories;

namespace DiscordBot.Services
{
    public class SettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<Settings> GetSettingsAsync(int guildId)
        {
            return await _settingsRepository.GetByGuildIdAsync(guildId);
        }

        public async Task UpdateSettingsAsync(Settings settings)
        {
            await _settingsRepository.AddAsync(settings); 
            await _settingsRepository.SaveAsync();       
        }
    }
}