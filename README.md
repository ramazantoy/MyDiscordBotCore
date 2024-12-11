# Discord Bot Core

DiscordBotCore is a customizable Discord bot developed with .NET 6 and designed to support multiple servers. It integrates seamlessly with MySQL for efficient server and settings management.

## Features
- **Multi-Server Support**: Separate configurations for each Discord server.
- **Channel Management**: Automatically creates `welcome` and `rin-settings` channels.
- **Easy Configuration**: Flexible settings management with MySQL integration.
- **Commands**:
  - `!setwelcome`: Enable or disable the welcome message.
  - `!setlevel`: Set the server's security level.
- **Status and Activity Support**: Configure bot status (online, do not disturb) and activities.

## Requirements
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [MySQL](https://dev.mysql.com/downloads/)
- A bot token from the Discord Developer Portal.

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/ramazantoy/DiscordBotCore.git
cd DiscordBotCore
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Database Configuration

1. Create a database in MySQL:

```sql
CREATE DATABASE discordbot;
```

2. Create your `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=discordbot;User=root;Password=yourpassword;"
  },
  "DiscordToken": "your-discord-bot-token"
}
```

3. Apply migrations to set up the database tables:

```bash
dotnet ef database update
```

### 4. Start the Bot

```bash
dotnet run
```

## Usage

1. Add your bot to a server via the Discord Developer Portal.
2. Test the basic functionality of the bot with the following commands:
   - `!setwelcome on` or `!setwelcome off`
   - `!setlevel 0` (Set a security level between 0 and 3.)



This project is licensed under the [MIT License](LICENSE).



