using Discord.Commands;
using DiscordBot.Services;

public class SettingsModule : ModuleBase<SocketCommandContext>
{
    private readonly SettingsService _settingsService;

    public SettingsModule(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [Command("setwelcome")]
    public async Task SetWelcomeMessageAsync(bool isEnabled)
    {
        var guildId = (int)Context.Guild.Id;
        var settings = await _settingsService.GetSettingsAsync(guildId);
        settings.WelcomeMessageEnabled = isEnabled;

        await _settingsService.UpdateSettingsAsync(settings);
        await ReplyAsync($"Hoş geldin mesajı ayarlandı: {(isEnabled ? "Açık" : "Kapalı")}");
    }

    [Command("setsecuritylevel")]
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
        await ReplyAsync($"Güvenlik seviyesi ayarlandı: {level}");
    }
}