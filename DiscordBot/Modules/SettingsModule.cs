using Discord;
using Discord.Commands;
using DiscordBot.Entities;
using DiscordBot.Services;

namespace DiscordBot.Modules;

public class SettingsModule : ModuleBase<SocketCommandContext>
{
    private readonly SettingsService _settingsService;

    public SettingsModule(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [Command("setwelcome")]
    public async Task SetWelcomeMessageAsync(string state)
    {
        try
        {
            // "on" veya "off" kontrolü
            bool isEnabled = state.ToLower() switch
            {
                "on" => true,
                "off" => false,
                _ => throw new ArgumentException("Geçersiz parametre! Lütfen 'on' veya 'off' kullanın.")
            };

            var guildId = (int)Context.Guild.Id;

            var settings = await _settingsService.GetSettingsAsync(guildId);
            if ( settings==null || settings.Id==0)
            {
                settings = new Settings { GuildId = guildId };
            }

            settings.WelcomeMessageEnabled = isEnabled;
            await _settingsService.UpdateSettingsAsync(settings);

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
        if (level < 0 || level > 3)
        {
            await ReplyAsync("Güvenlik seviyesi 0-3 arasında olmalıdır.");
            return;
        }

        var guildId = (int)Context.Guild.Id;
        var settings = await _settingsService.GetSettingsAsync(guildId);
        settings.SecurityLevel = level;

        await _settingsService.UpdateSettingsAsync(settings);
        await ReplyAsync(
            $"Güvenlik seviyesi ayarlandı: {level}",
            messageReference: new MessageReference(Context.Message.Id) 
        );

        
    }
}