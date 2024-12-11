using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Data;
using DiscordBot.Entities;
using DiscordBot.Interfaces;
using DiscordBot.Services;
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
    private readonly SettingsService _settingsService;
    private readonly GuildService _guildService;

    public Bot(ILogger<Bot> logger, IConfiguration configuration, DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider, SettingsService settingsService, GuildService guildService)
    {
        _logger = logger;
        _configuration = configuration;
        _client = client;
        _commands = commands;
        _serviceProvider = serviceProvider;
        _settingsService = settingsService;
        _guildService = guildService;

        // Eventlere abone ol
        _client.Log += LogDiscordMessages;
        _client.MessageReceived += HandleCommandAsync;
        _client.UserJoined += OnUserJoinedAsync;
        _client.GuildAvailable += OnGuildAvailableAsync;
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
            var guildId = context.Guild.Id;
            var guild =await _guildService.GetGuildById(guildId);
            var settings = await _settingsService.GetSettingsByGuildIdAsync(guild.Id);
            using var scope = _serviceProvider.CreateScope();
            
            if (!settings.SettingsChannelId.HasValue || context.Channel.Id != settings.SettingsChannelId.Value)   return;
            
            var result = await _commands.ExecuteAsync(context, argPos, _serviceProvider);

            if (!result.IsSuccess)
            {
                _logger.LogError(result.ErrorReason);
            }
        }
    }
    
    private async Task OnUserJoinedAsync(SocketGuildUser user)
    {
        var welcomeChannel = user.Guild.TextChannels.FirstOrDefault(c => c.Name == "welcome");
        _logger.LogInformation($"User joined: {user.Username} in guild {user.Guild.Name}");

        if (welcomeChannel == null)
        {
            _logger.LogWarning($"Welcome channel not found in guild: {user.Guild.Name}. Creating one...");
            
            var restChannel = await user.Guild.CreateTextChannelAsync("welcome", properties =>
            {
                properties.Topic = "Yeni katılan üyeler burada karşılanır.";
                properties.PermissionOverwrites = new Overwrite[]
                {
                    new Overwrite(user.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(sendMessages: PermValue.Deny)),
                    new Overwrite(user.Guild.Roles.FirstOrDefault(r => r.Permissions.Administrator)?.Id ?? 0, PermissionTarget.Role, new OverwritePermissions(sendMessages: PermValue.Allow))
                };
            });
            
            welcomeChannel = user.Guild.GetTextChannel(restChannel.Id);

            _logger.LogInformation($"Welcome channel created: {welcomeChannel.Name} in guild {user.Guild.Name}");
        }
        
        await welcomeChannel.SendMessageAsync($"Hoş geldin, {user.Mention}! Sunucumuza katıldığın için mutluyuz.");
    }



private async Task OnGuildAvailableAsync(SocketGuild guild)
{
    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    var existingGuild = await dbContext.Guilds.Include(g => g.Settings).FirstOrDefaultAsync(g => g.DiscordId == guild.Id);
    if (existingGuild == null)
    {
        existingGuild = new Guild
        {
            DiscordId = guild.Id,
            Name = guild.Name,
            CreatedAt = DateTime.UtcNow,
            Settings = new Settings
            {
                Guild = existingGuild,
                SecurityLevel = 0,
                WelcomeMessageEnabled = true
            }
        };

        dbContext.Guilds.Add(existingGuild);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation($"Guild added: {guild.Name} with default settings.");
    }

    var settings = existingGuild.Settings;
    
    if (!settings.SettingsChannelId.HasValue) 
    {
        ITextChannel? settingsChannel = guild.TextChannels.FirstOrDefault(c => c.Name == "rin-settings");
        if (settingsChannel == null) 
        {
            try
            {
                var newChannel = await guild.CreateTextChannelAsync("rin-settings", props =>
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

                settingsChannel = guild.GetTextChannel(newChannel.Id);
                settings.SettingsChannelId = newChannel.Id;

                _logger.LogInformation($"Settings channel created: {settingsChannel?.Name} in guild {guild.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while creating settings channel in guild {guild.Name}");
            }
        }
        else
        {
        
            settings.SettingsChannelId = settingsChannel.Id;
            _logger.LogInformation($"Settings channel found: {settingsChannel.Name} in guild {guild.Name}");
        }
    }


    if (!settings.WelcomeChannelId.HasValue) 
    {
        ITextChannel? welcomeChannel = guild.TextChannels.FirstOrDefault(c => c.Name == "welcome");
        if (welcomeChannel == null) 
        {
            try
            {
                var newChannel = await guild.CreateTextChannelAsync("welcome", props =>
                {
                    props.Topic = "Yeni katılan üyeler burada karşılanır.";
                    props.PermissionOverwrites = new List<Overwrite>
                    {
                        new Overwrite(
                            guild.EveryoneRole.Id,
                            PermissionTarget.Role,
                            new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny)
                        ),
                        new Overwrite(
                            guild.Roles.FirstOrDefault(r => r.Permissions.Administrator)?.Id ?? 0,
                            PermissionTarget.Role,
                            new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow)
                        )
                    };
                });

                welcomeChannel = guild.GetTextChannel(newChannel.Id);
                settings.WelcomeChannelId = newChannel.Id;

                _logger.LogInformation($"Welcome channel created: {welcomeChannel?.Name} in guild {guild.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while creating welcome channel in guild {guild.Name}");
            }
        }
        else
        {
            settings.WelcomeChannelId = welcomeChannel.Id;
            _logger.LogInformation($"Welcome channel found: {welcomeChannel.Name} in guild {guild.Name}");
        }
    }
    
    dbContext.Guilds.Update(existingGuild);
    await dbContext.SaveChangesAsync();

    _logger.LogInformation($"Guild updated: {guild.Name}");
}





    private Task LogDiscordMessages(LogMessage log)
    {
        _logger.LogInformation(log.ToString());
        return Task.CompletedTask;
    }
}
