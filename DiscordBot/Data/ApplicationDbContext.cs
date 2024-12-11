using DiscordBot.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "Server=localhost;Database=discordbot;User=root;Password=;",
                new MySqlServerVersion(new Version(8, 0, 25))
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guild>()
                .HasOne(g => g.Settings)
                .WithOne(s => s.Guild)
                .HasForeignKey<Settings>(s => s.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
            
        
            modelBuilder.Entity<Role>()
                .HasOne(r => r.Guild)
                .WithMany(g => g.Roles)
                .HasForeignKey(r => r.GuildId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}