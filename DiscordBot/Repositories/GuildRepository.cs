using DiscordBot.Data;
using DiscordBot.Entities;
using DiscordBot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Repositories;

public class GuildRepository :Repository<Guild>, IGuildRepository
{
    private readonly ApplicationDbContext _context;
    public GuildRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<Guild?> GetByDiscordIdAsync(ulong discordId)
    {
        return await _context.Guilds.FirstOrDefaultAsync(g => g.DiscordId == discordId);
    }
    
    public async Task<List<Guild>> GetAllGuildsAsync()
    {
        return await _context.Guilds.ToListAsync();
    }
}