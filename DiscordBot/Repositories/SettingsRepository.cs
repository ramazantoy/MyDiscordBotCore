using DiscordBot.Data;
using DiscordBot.Entities;
using DiscordBot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Repositories
{


    public class SettingsRepository : Repository<Settings>, ISettingsRepository
    {
        private readonly ApplicationDbContext _context;

        public SettingsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Settings> GetByGuildIdAsync(int guildId)
        {
            return await _context.Settings.FirstOrDefaultAsync(s => s.GuildId == guildId)
                   ?? new Settings { GuildId = guildId, SecurityLevel = 0, WelcomeMessageEnabled = true };
        }
    }
}