using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApi.Application.Constants;
using WebApi.Application.Services;
using WebApi.Core.Entities;
using WebApi.Tests.Integration.Helpers;

namespace WebApi.Tests.Integration.SignalR;

[Collection("Integration Tests")]
public class AppHubChatTests : BaseIntegrationTest, IAsyncLifetime
{
    public AppHubChatTests(IntegrationTestFixture fixture) : base(fixture) {}

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await CleanupConnectionAsync();
        await Task.Delay(200);
    }

    private HubConnection _hubConnection = null!;
    private string _roomHash = null!;
    private string _jwtToken = null!;

    private async Task SetupConnectionAsync()
    {
        var db = Factory.GetDbContext();
        var room = new Room("TestRoom", "");
        room.Users.Add(new User("TestUser", Role.Admin) { RoomHash = room.Hash });
        room.Users.Add(new User("RegularUser", Role.User) { RoomHash = room.Hash });
        db.Rooms.Add(room);
        await db.SaveChangesAsync();
        _roomHash = room.Hash!;

        var jwtService = Factory.GetRequiredService<IJwtTokenService>();
        _jwtToken = jwtService.GenerateToken("TestUser", Role.Admin, _roomHash);

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost/Hub/Room", options =>
            {
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.Headers.Add("Authorization", $"Bearer {_jwtToken}");
            })
            .Build();
    }

    private async Task CleanupConnectionAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.StopAsync().ConfigureAwait(false);
                }
            }
            catch { }

            try
            {
                await _hubConnection.DisposeAsync().ConfigureAwait(false);
            }
            catch { }
        }
    }

    [Fact]
    public async Task AddChatMessage_ValidMessage_ShouldBroadcastToGroup()
    {
        Factory.ResetSingletonState();
        await SetupConnectionAsync();
        await _hubConnection.StartAsync();

        var chatMessage = new ChatMessage
        {
            Username = "TestUser",
            Text = "Hello",
            Date = DateTime.UtcNow
        };

        bool receivedMessage = false;
        _hubConnection.On(HubMessages.OnAddChatMessage, (ChatMessage received) =>
        {
            if (received.Text == "Hello")
            {
                receivedMessage = true;
            }
        });

        await _hubConnection.InvokeAsync(HubMessages.AddChatMessage, _roomHash, chatMessage);
        await Task.Delay(100);

        receivedMessage.Should().BeTrue();

        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.ChatMessages).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        room.Should().NotBeNull();
        room!.ChatMessages.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddChatMessage_MessageTooLong_ShouldNotAdd()
    {
        Factory.ResetSingletonState();
        await SetupConnectionAsync();
        await _hubConnection.StartAsync();

        var chatMessage = new ChatMessage
        {
            Username = "TestUser",
            Text = new string('A', 201),
            Date = DateTime.UtcNow
        };

        await _hubConnection.InvokeAsync(HubMessages.AddChatMessage, _roomHash, chatMessage);
        await Task.Delay(100);

        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.ChatMessages).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        room!.ChatMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task AddChatMessage_WithoutPermission_ShouldNotAdd()
    {
        Factory.ResetSingletonState();
        await SetupConnectionAsync();
        await _hubConnection.StopAsync();

        var userToken = GenerateJwtToken("RegularUser", Role.User, _roomHash);
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost/Hub/Room", options =>
            {
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.Headers.Add("Authorization", $"Bearer {userToken}");
            })
            .Build();

        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.UserPermissions).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        room!.UserPermissions.CanAddChatMessage = false;
        await db.SaveChangesAsync();
        await Task.Delay(100);

        await _hubConnection.StartAsync();

        var chatMessage = new ChatMessage
        {
            Username = "RegularUser",
            Text = "Hello",
            Date = DateTime.UtcNow
        };

        await _hubConnection.InvokeAsync(HubMessages.AddChatMessage, _roomHash, chatMessage);
        await Task.Delay(100);

        var updatedRoom = await db.Rooms.Include(r => r.ChatMessages).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        updatedRoom!.ChatMessages.Should().BeEmpty();

        await _hubConnection.StopAsync();
        await CleanupConnectionAsync();
    }

    private string GenerateJwtToken(string username, string role, string roomHash)
    {
        return Factory.GetRequiredService<IJwtTokenService>().GenerateToken(username, role, roomHash);
    }
}
