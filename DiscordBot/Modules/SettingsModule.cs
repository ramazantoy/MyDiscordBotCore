using Discord;
using Discord.Commands;
using DiscordBot.Entities;
using DiscordBot.Services;

namespace DiscordBot.Modules;

public class SettingsModule : ModuleBase<SocketCommandContext>
{
    private readonly SettingsService _settingsService;
    private readonly GuildService _guildService;

    public SettingsModule(SettingsService settingsService, GuildService guildService)
    {
        _settingsService = settingsService;
        _guildService = guildService;
    }

    private async Task<Guild> GetGuild(ulong guildId)
    {
        return await _guildService.GetGuildById(guildId);
    }

    private async Task<Settings> GetSettings(int id)
    {
        return await _settingsService.GetSettingsByGuildIdAsync(id);
    }


    
    [Command("setwelcome")]
    public async Task SetWelcomeMessageAsync(string state)
    {
        try
        {
            var isEnabled = state.ToLower() switch
            {
                "on" => true,
                "off" => false,
                _ => throw new ArgumentException("Geçersiz parametre! Lütfen 'on' veya 'off' kullanın.")
            };
            
            var guild = await GetGuild(Context.Guild.Id);
            var settings = await GetSettings(guild.Id);
            

            settings.WelcomeMessageEnabled = isEnabled;
            
            await _settingsService.UpdateSettingsAsync(settings, guild);
            
            await ReplyAsync(
                $"Hoş geldin mesajı {(isEnabled ? "aktif edildi" : "devre dışı bırakıldı")}.",
                messageReference: new MessageReference(Context.Message.Id)
            );
        }
        catch (ArgumentException ex)
        {
            await ReplyAsync(ex.Message, messageReference: new MessageReference(Context.Message.Id));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bir hata oluştu: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            await ReplyAsync("Bir hata oluştu, lütfen tekrar deneyin.");
        }
    }

    [Command("setlevel")]
    public async Task SetSecurityLevelAsync(int level)
    {
        try
        {
            if (level < 0 || level > 3)
            {
                await ReplyAsync("Güvenlik seviyesi 0-3 arasında olmalıdır.");
                return;
            }
            
            var guild = await GetGuild(Context.Guild.Id);
            var settings = await GetSettings(guild.Id);
            
            if(guild==null) return;

            settings.SecurityLevel = level;
            
            await _settingsService.UpdateSettingsAsync(settings, guild);
            
            await ReplyAsync(
                $"Güvenlik seviyesi ayarlandı: {level}",
                messageReference: new MessageReference(Context.Message.Id)
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bir hata oluştu: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            await ReplyAsync("Bir hata oluştu, lütfen tekrar deneyin.");
        }
    }
}
