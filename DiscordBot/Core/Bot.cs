using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Data;
using DiscordBot.Entities;
using DiscordBot.Interfaces;
using DiscordBot.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Core;

public class Bot : IBot
{
    private readonly ILogger<Bot> _logger;
    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _serviceProvider;

    public Bot(ILogger<Bot> logger, IConfiguration configuration, DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _client = client;
        _commands = commands;
        _serviceProvider = serviceProvider;

        // Eventlere abone ol
        _client.Log += LogDiscordMessages;
        _client.MessageReceived += HandleCommandAsync;
        _client.UserJoined += OnUserJoinedAsync;
        _client.GuildAvailable += OnGuildAvailableAsync;
    }

    public async Task StartAsync(IServiceProvider services)
    {
        var discordToken = _configuration["DiscordToken"] ?? throw new Exception("Missing Discord token");

        _logger.LogInformation("Starting bot...");

        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);
        await _client.LoginAsync(TokenType.Bot, discordToken);
        await _client.StartAsync();

        _logger.LogInformation("Bot started successfully.");
    }

    public async Task StartAsync(ServiceProvider services)
    {
        var discordToken = _configuration["DiscordToken"] ?? throw new Exception("Missing Discord token");

        _logger.LogInformation("Starting bot...");

        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);
        await _client.LoginAsync(TokenType.Bot, discordToken);
        await _client.StartAsync();

        _logger.LogInformation("Bot started successfully.");
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Shutting down bot...");

        await _client.LogoutAsync();
        await _client.StopAsync();

        _logger.LogInformation("Bot shut down successfully.");
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        if (arg is not SocketUserMessage message || message.Author.IsBot)
            return;

        var context = new SocketCommandContext(_client, message);

        var argPos = 0;
        if (message.HasCharPrefix('!', ref argPos))
        {
            var result = await _commands.ExecuteAsync(context, argPos, _serviceProvider);

            if (!result.IsSuccess)
                _logger.LogError(result.ErrorReason);
        }
    }

    private async Task OnUserJoinedAsync(SocketGuildUser user)
    {
        var welcomeChannel = user.Guild.TextChannels.FirstOrDefault(c => c.Name == "welcome");
        _logger.LogInformation($"User joined: {user.Username} in guild {user.Guild.Name}");

        if (welcomeChannel != null)
        {
            await welcomeChannel.SendMessageAsync($"Hoş geldin, {user.Mention}! Sunucumuza katıldığın için mutluyuz.");
        }
        else
        {
            _logger.LogWarning($"Welcome channel not found in guild: {user.Guild.Name}");
        }
    }

    private async Task OnGuildAvailableAsync(SocketGuild guild)
{
    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
    
    ITextChannel? settingsChannel = guild.TextChannels.FirstOrDefault(c => c.Name == "bot-settings");
    if (settingsChannel == null)
    {
        var newChannel = await guild.CreateTextChannelAsync("bot-settings", props =>
        {
            props.Topic = "Bu kanal, botun ayarlarını yapmak için kullanılır.";
            props.PermissionOverwrites = new List<Overwrite>
            {
                new Overwrite(
                    guild.EveryoneRole.Id,
                    PermissionTarget.Role,
                    new OverwritePermissions(viewChannel: PermValue.Deny)
                ),
                new Overwrite(
                    guild.Roles.FirstOrDefault(r => r.Permissions.Administrator)?.Id ?? 0,
                    PermissionTarget.Role,
                    new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow)
                )
            };
        });

        if (newChannel != null)
        {
            settingsChannel = newChannel;
            _logger.LogInformation($"Settings channel created: {settingsChannel.Name} in guild {guild.Name}");
        }
        else
        {
            _logger.LogError($"Failed to create settings channel in guild {guild.Name}");
            return;
        }
    }

    // Sunucuyu kontrol et
    var existingGuild = await dbContext.Guilds.FirstOrDefaultAsync(g => g.DiscordId == guild.Id);
    if (existingGuild == null)
    {
        // Yeni guild ve varsayılan ayarları ekle
        dbContext.Guilds.Add(new Guild
        {
            DiscordId = guild.Id,
            Name = guild.Name,
            SettingsChannelId = settingsChannel?.Id, // NULL kontrolü
            CreatedAt = DateTime.UtcNow,
            Settings = new Settings
            {
                SecurityLevel = 0,
                WelcomeMessageEnabled = true
            }
        });
        await dbContext.SaveChangesAsync();

        _logger.LogInformation($"Guild added: {guild.Name} with default settings.");
    }
    else
    {
        // SettingsChannelId kontrolü ve güncelleme
        if (settingsChannel != null)
        {
            existingGuild.SettingsChannelId = settingsChannel.Id;
        }

        await dbContext.SaveChangesAsync();

        _logger.LogInformation($"Guild updated: {guild.Name}");
    }
}


    private Task LogDiscordMessages(LogMessage log)
    {
        _logger.LogInformation(log.ToString());
        return Task.CompletedTask;
    }
}
