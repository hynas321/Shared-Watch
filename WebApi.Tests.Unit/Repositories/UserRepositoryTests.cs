using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Repositories;
using WebApi.Tests.Unit.Helpers;
using WebApi.Api.SignalR.Interfaces;
using WebApi.Core.Entities;
using WebApi.Application.Constants;

namespace WebApi.Tests.Unit.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IHubConnectionMapper> _mockHubConnectionMapper;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        _context = InMemoryDbContextFactory.CreateContext();
        _mockHubConnectionMapper = new Mock<IHubConnectionMapper>();
        _userRepository = new UserRepository(_context, _mockHubConnectionMapper.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<string> CreateTestRoomAsync()
    {
        var room = new Room("TestRoom", "");
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room.Hash!;
    }

    [Fact]
    public async Task AddUserAsync_ValidUser_ShouldAddUserToRoom()
    {
        var roomHash = await CreateTestRoomAsync();
        var user = new User("TestUser", Role.User);

        var result = await _userRepository.AddUserAsync(roomHash, user, TestHelpers.GetCancellationToken());

        result.Should().BeTrue();
        var room = await _context.Rooms.Include(r => r.Users).FirstAsync(r => r.Hash == roomHash);
        room.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserAsync_ExistingUser_ShouldReturnUser()
    {
        var roomHash = await CreateTestRoomAsync();
        var user = new User("TestUser", Role.User);
        await _userRepository.AddUserAsync(roomHash, user, TestHelpers.GetCancellationToken());

        var result = await _userRepository.GetUserAsync(roomHash, "TestUser", TestHelpers.GetCancellationToken());

        result.Should().NotBeNull();
        result!.Username.Should().Be("TestUser");
    }

    [Fact]
    public async Task DeleteUserByUsernameAsync_ExistingUser_ShouldReturnDeletedUser()
    {
        var roomHash = await CreateTestRoomAsync();
        var user = new User("TestUser", Role.User);
        await _userRepository.AddUserAsync(roomHash, user, TestHelpers.GetCancellationToken());

        var result = await _userRepository.DeleteUserByUsernameAsync(roomHash, "TestUser", TestHelpers.GetCancellationToken());

        result.Should().NotBeNull();
        result!.Username.Should().Be("TestUser");
        var room = await _context.Rooms.Include(r => r.Users).FirstAsync(r => r.Hash == roomHash);
        room.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateUserAsync_ValidUser_ShouldReturnTrue()
    {
        var roomHash = await CreateTestRoomAsync();
        var user = new User("TestUser", Role.User);
        await _userRepository.AddUserAsync(roomHash, user, TestHelpers.GetCancellationToken());

        user.Role = Role.Admin;
        var result = await _userRepository.UpdateUserAsync(user, TestHelpers.GetCancellationToken());

        result.Should().BeTrue();
        var updatedUser = await _userRepository.GetUserAsync(roomHash, "TestUser", TestHelpers.GetCancellationToken());
        updatedUser!.Role.Should().Be(Role.Admin);
    }

    [Fact]
    public async Task GetUsersDTOAsync_ShouldReturnCorrectDTOs()
    {
        var roomHash = await CreateTestRoomAsync();
        var user1 = new User("AdminUser", Role.Admin);
        var user2 = new User("RegularUser", Role.User);
        await _userRepository.AddUserAsync(roomHash, user1, TestHelpers.GetCancellationToken());
        await _userRepository.AddUserAsync(roomHash, user2, TestHelpers.GetCancellationToken());

        var result = await _userRepository.GetUsersDTOAsync(roomHash, TestHelpers.GetCancellationToken());

        result.Should().HaveCount(2);
        result.Should().Contain(dto => dto.Username == "AdminUser" && dto.IsAdmin == true);
    }

    [Fact]
    public async Task AddUserAsync_MultipleUsers_ShouldAddAllUsers()
    {
        var roomHash = await CreateTestRoomAsync();
        await _userRepository.AddUserAsync(roomHash, new User("User1", Role.User), TestHelpers.GetCancellationToken());
        await _userRepository.AddUserAsync(roomHash, new User("User2", Role.User), TestHelpers.GetCancellationToken());
        await _userRepository.AddUserAsync(roomHash, new User("User3", Role.Admin), TestHelpers.GetCancellationToken());

        var room = await _context.Rooms.Include(r => r.Users).FirstAsync(r => r.Hash == roomHash);
        room.Users.Should().HaveCount(3);
    }
}
