using DotnetServer.Core.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<PlaylistVideo> PlaylistVideos { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserPermissions> UserPermissions { get; set; }
    public DbSet<VideoPlayer> VideoPlayers { get; set; }
    public DbSet<RoomSettings> RoomSettings { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Room>()
             .HasMany(r => r.ChatMessages)
             .WithOne()
             .HasForeignKey(cm => cm.RoomHash)
             .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Room>()
            .HasMany(r => r.PlaylistVideos)
            .WithOne()
            .HasForeignKey(pv => pv.RoomHash)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Room>()
            .HasMany(r => r.Users)
            .WithOne()
            .HasForeignKey(u => u.RoomHash)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Room>()
            .HasOne(r => r.RoomSettings)
            .WithOne()
            .HasForeignKey<RoomSettings>(rs => rs.RoomHash)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Room>()
            .HasOne(r => r.UserPermissions)
            .WithOne()
            .HasForeignKey<UserPermissions>(up => up.RoomHash)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Room>()
            .HasOne(r => r.VideoPlayer)
            .WithOne()
            .HasForeignKey<VideoPlayer>(vp => vp.RoomHash)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .HasKey(cm => new { cm.Username, cm.Date });

        modelBuilder.Entity<Room>()
            .HasIndex(r => r.Hash)
            .IsUnique();
    }
}
