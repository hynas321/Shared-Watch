using Microsoft.EntityFrameworkCore;
using WebApi.Core.Entities;
using WebApi.Application.Constants;

namespace WebApi.Tests.Unit.Helpers;

public static class InMemoryDbContextFactory
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var context = new AppDbContext(options);
        return context;
    }

    public static AppDbContext CreateContextWithData()
    {
        var context = CreateContext();
        SeedData(context);
        return context;
    }

    private static void SeedData(AppDbContext context)
    {
        var room = new Room("TestRoom", "")
        {
            Hash = "test-room-hash",
            RoomSettings = new RoomSettings
            {
                RoomHash = "test-room-hash",
                RoomName = "TestRoom",
                MaxUsers = 10,
                RoomPassword = ""
            },
            UserPermissions = new UserPermissions
            {
                RoomHash = "test-room-hash",
                CanAddChatMessage = true,
                CanAddVideo = true
            },
            Users = new List<User>
            {
                new("User1", Role.Admin) { RoomHash = "test-room-hash" },
                new("User2", Role.User) { RoomHash = "test-room-hash" }
            },
            ChatMessages = new List<ChatMessage>
            {
                new() { Username = "User1", Text = "Hello", Date = DateTime.UtcNow, RoomHash = "test-room-hash" }
            },
            PlaylistVideos = new List<PlaylistVideo>
            {
                new() { Hash = "video1", Url = "https://youtube.com/watch?v=test1", RoomHash = "test-room-hash" }
            }
        };

        context.Rooms.Add(room);
        context.SaveChanges();
    }
}
