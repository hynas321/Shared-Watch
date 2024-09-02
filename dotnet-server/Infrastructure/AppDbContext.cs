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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Server=localhost;Database=mydatabase;User Id=postgres;Password=password;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Room -> ChatMessages (One-to-Many)
        modelBuilder.Entity<Room>()
             .HasMany(r => r.ChatMessages)
             .WithOne()
             .HasForeignKey(cm => cm.RoomHash)
             .OnDelete(DeleteBehavior.Cascade);

        // Room -> PlaylistVideos (One-to-Many)
        modelBuilder.Entity<Room>()
            .HasMany(r => r.PlaylistVideos)
            .WithOne()
            .HasForeignKey(pv => pv.RoomHash)
            .OnDelete(DeleteBehavior.Cascade);

        // Room -> Users (One-to-Many)
        modelBuilder.Entity<Room>()
            .HasMany(r => r.Users)
            .WithOne()
            .HasForeignKey(u => u.RoomHash)
            .OnDelete(DeleteBehavior.Cascade);

        // Room -> RoomSettings (One-to-One)
        modelBuilder.Entity<Room>()
            .HasOne(r => r.RoomSettings)
            .WithOne()
            .HasForeignKey<RoomSettings>(rs => rs.RoomHash)
            .OnDelete(DeleteBehavior.Cascade);

        // Room -> UserPermissions (One-to-One)
        modelBuilder.Entity<Room>()
            .HasOne(r => r.UserPermissions)
            .WithOne()
            .HasForeignKey<UserPermissions>(up => up.RoomHash)
            .OnDelete(DeleteBehavior.Cascade);

        // Room -> VideoPlayer (One-to-One)
        modelBuilder.Entity<Room>()
            .HasOne(r => r.VideoPlayer)
            .WithOne()
            .HasForeignKey<VideoPlayer>(vp => vp.RoomHash)
            .OnDelete(DeleteBehavior.Cascade);

        // Set composite key for ChatMessage
        modelBuilder.Entity<ChatMessage>()
            .HasKey(cm => new { cm.Username, cm.Date });

        // Set unique constraint on Room Hash (if not handled automatically by [Key])
        modelBuilder.Entity<Room>()
            .HasIndex(r => r.Hash)
            .IsUnique();
    }
}
