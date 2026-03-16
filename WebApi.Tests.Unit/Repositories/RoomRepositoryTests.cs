using WebApi.Infrastructure.Repositories;
using WebApi.Tests.Unit.Helpers;
using WebApi.Core.Entities;
using WebApi.Core.Enums;

namespace WebApi.Tests.Unit.Repositories;

public class RoomRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RoomRepository _roomRepository;

    public RoomRepositoryTests()
    {
        _context = InMemoryDbContextFactory.CreateContext();
        _roomRepository = new RoomRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task AddRoomAsync_ValidRoom_ShouldAddRoomToDatabase()
    {
        var room = new Room("TestRoom", "");
        var result = await _roomRepository.AddRoomAsync(room, TestHelpers.GetCancellationToken());

        result.Should().BeTrue();
        _context.Rooms.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetRoomAsync_ExistingRoomHash_ShouldReturnRoom()
    {
        var room = new Room("TestRoom", "");
        await _roomRepository.AddRoomAsync(room, TestHelpers.GetCancellationToken());

        var result = await _roomRepository.GetRoomAsync(room.Hash!, TestHelpers.GetCancellationToken());

        result.Should().NotBeNull();
        result!.Hash.Should().Be(room.Hash);
    }

    [Fact]
    public async Task DeleteRoomAsync_ExistingRoom_ShouldReturnDeletedRoom()
    {
        var room = new Room("TestRoom", "");
        await _roomRepository.AddRoomAsync(room, TestHelpers.GetCancellationToken());

        var result = await _roomRepository.DeleteRoomAsync(room.Hash!, TestHelpers.GetCancellationToken());

        result.Should().NotBeNull();
        _context.Rooms.Count().Should().Be(0);
    }

    [Fact]
    public async Task UpdateRoomAsync_ValidRoom_ShouldReturnTrue()
    {
        var room = new Room("TestRoom", "");
        await _roomRepository.AddRoomAsync(room, TestHelpers.GetCancellationToken());

        room.RoomSettings.MaxUsers = 20;
        var result = await _roomRepository.UpdateRoomAsync(room, TestHelpers.GetCancellationToken());

        result.Should().BeTrue();
        var updatedRoom = await _roomRepository.GetRoomAsync(room.Hash!, TestHelpers.GetCancellationToken());
        updatedRoom!.RoomSettings.MaxUsers.Should().Be(20);
    }

    [Fact]
    public async Task GetRoomsAsync_ShouldReturnAllRooms()
    {
        var room1 = new Room("Room1", "");
        var room2 = new Room("Room2", "");
        await _roomRepository.AddRoomAsync(room1, TestHelpers.GetCancellationToken());
        await _roomRepository.AddRoomAsync(room2, TestHelpers.GetCancellationToken());

        var result = await _roomRepository.GetRoomsAsync(TestHelpers.GetCancellationToken());

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddRoomAsync_PrivateRoom_ShouldSetCorrectRoomType()
    {
        var room = new Room("PrivateRoom", "password123");
        await _roomRepository.AddRoomAsync(room, TestHelpers.GetCancellationToken());

        var result = await _roomRepository.GetRoomAsync(room.Hash!, TestHelpers.GetCancellationToken());
        result!.RoomSettings.RoomType.Should().Be(RoomTypes.Private);
    }
}
