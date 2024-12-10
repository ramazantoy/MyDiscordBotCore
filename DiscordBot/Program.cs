using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Core;
using DiscordBot.Data;
using DiscordBot.Interfaces;
using DiscordBot.Repositories;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ISettingsRepository = DiscordBot.Interfaces.ISettingsRepository;

namespace DiscordBot;

public static class Program
{
    public static async Task Main(string[] args)
    {
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) 
            .Build();


        var discordToken = configuration["DiscordToken"];
        if (string.IsNullOrEmpty(discordToken))
        {
            throw new Exception("DiscordToken is missing in appsettings.json or UserSecrets.");
        }

        Console.WriteLine("Configuration loaded successfully.");
        
        var serviceProvider = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                });
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .AddSingleton<IConfiguration>(configuration)
            .AddDbContext<ApplicationDbContext>() // DbContext
            .AddScoped<ISettingsRepository, SettingsRepository>() // Settings Repository
            .AddScoped<SettingsService>() // Settings Service
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers
            }))
            .AddSingleton<CommandService>()
            .AddScoped<IBot, Bot>() // Bot
            .BuildServiceProvider();

        try
        {
          
            var bot = serviceProvider.GetRequiredService<IBot>();
            await bot.StartAsync(serviceProvider);

            Console.WriteLine("Bot is running. Press 'Q' to quit.");

            do
            {
                var keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Q)
                {
                    Console.WriteLine("\nShutting down...");
                    await bot.StopAsync();
                    break;
                }
            } while (true);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error: {exception.Message}");
        }
    }
}
