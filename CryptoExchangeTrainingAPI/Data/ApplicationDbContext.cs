using Microsoft.EntityFrameworkCore;
using CryptoExchangeTrainingAPI.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CryptoExchangeTrainingAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        // Конструктор для передачи параметров конфигурации
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet для каждой таблицы
        public DbSet<Trade> Trades { get; set; } = null!;
        public DbSet<MarketData> MarketData { get; set; } = null!;
        public DbSet<UserAsset> UserAssets { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<HistoricalData> HistoricalData { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Token> Tokens { get; set; } = null!;

        public new Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        // Настройка моделей с помощью Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Trades)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserAssets)
                .WithOne(ua => ua.User)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}