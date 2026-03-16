using Microsoft.Extensions.DependencyInjection;
using WebApi.Application.HostedServices;
using WebApi.Tests.Unit.Helpers;
using WebApi.Core.Entities;
using WebApi.Application.Constants;

namespace WebApi.Tests.Unit.HostedServices;

public class DatabaseCleanupTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<ILogger<DatabaseCleanup>> _mockLogger;
    private readonly DatabaseCleanup _databaseCleanup;

    public DatabaseCleanupTests()
    {
        _context = InMemoryDbContextFactory.CreateContext();

        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockLogger = new Mock<ILogger<DatabaseCleanup>>();

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeFactory.Object);

        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(AppDbContext))).Returns(_context);

        _databaseCleanup = new DatabaseCleanup(_mockServiceProvider.Object, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task StartAsync_WithPopulatedDatabase_ShouldClearAllTables()
    {
        var room = new Room("TestRoom", "");
        room.Users.Add(new User("User1", Role.Admin));
        room.ChatMessages.Add(new ChatMessage { Username = "User1", Text = "Test", Date = DateTime.UtcNow });

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        await _databaseCleanup.StartAsync(CancellationToken.None);

        _context.Rooms.Should().BeEmpty();
        _context.Users.Should().BeEmpty();
        _context.ChatMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task StartAsync_WithEmptyDatabase_ShouldCompleteSuccessfully()
    {
        var action = async () => await _databaseCleanup.StartAsync(CancellationToken.None);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StartAsync_WithMultipleRoomsAndUsers_ShouldClearAllData()
    {
        var room1 = new Room("Room1", "");
        room1.Users.Add(new User("User1", Role.Admin));
        room1.Users.Add(new User("User2", Role.User));

        var room2 = new Room("Room2", "");
        room2.Users.Add(new User("User3", Role.Admin));

        _context.Rooms.AddRange(room1, room2);
        await _context.SaveChangesAsync();

        await _databaseCleanup.StartAsync(CancellationToken.None);

        _context.Rooms.Should().BeEmpty();
        _context.Users.Should().BeEmpty();
    }
}
