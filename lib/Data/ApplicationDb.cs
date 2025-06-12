using Microsoft.EntityFrameworkCore;
using Pixlmint.Aioniq.Model;

namespace Pixlmint.Aioniq.Data;

public class ApplicationDb : DbContext, UsersDbContext
{
    public ApplicationDb(DbContextOptions<ApplicationDb> options)
        : base(options) { }

    public DbSet<Calendar> Calendars => Set<Calendar>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserTokens> UserTokens => Set<UserTokens>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<User>()
            .HasOne(e => e.Tokens)
            .WithOne(e => e.User)
            .HasForeignKey<User>(e => e.UserTokensId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder
            .Entity<UserTokens>()
            .HasOne(e => e.User)
            .WithOne(e => e.Tokens)
            .HasForeignKey<UserTokens>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>().HasIndex(e => e.GoogleUserId).IsUnique(true);
        modelBuilder.Entity<User>().HasIndex(e => e.Email).IsUnique(true);

        modelBuilder.Entity<User>()
            .HasMany(e => e.RefreshTokens)
            .WithOne(e => e.User)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public interface UsersDbContext
{
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
