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

        public async Task<Settings> GetSettingsByGuildIdAsync(int guildId)
        {
            return await _settingsRepository.GetSettingsByGuildIdAsync(guildId);
        }
        
        public async Task SetWelcomeChannelIdAsync(int guildId, ulong channelId)
        {
            var settings = await _settingsRepository.GetSettingsByGuildIdAsync(guildId);
            if (settings == null)
            {
                settings = new Settings { GuildId = guildId };
                await _settingsRepository.AddAsync(settings);
            }

            settings.WelcomeChannelId = channelId;
            await _settingsRepository.SaveAsync();
        }


        public async Task UpdateSettingsAsync(Settings settings, Guild guild)
        {
            var existingSettings = await _settingsRepository.GetSettingsByGuildIdAsync(guild.Id);

            if (existingSettings != null)
            {
                existingSettings.WelcomeMessageEnabled = settings.WelcomeMessageEnabled;
                existingSettings.SecurityLevel = settings.SecurityLevel;
                existingSettings.SettingsChannelId = settings.SettingsChannelId;
                existingSettings.WelcomeChannelId = settings.WelcomeChannelId;
                _settingsRepository.Update(existingSettings);
            }
            else
            {
                settings.GuildId = guild.Id; 
                settings.Guild = guild;     

                await _settingsRepository.AddAsync(settings);
            }
            
            await _settingsRepository.SaveAsync();
        }


    }
}