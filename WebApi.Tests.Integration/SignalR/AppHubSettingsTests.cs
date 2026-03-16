using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Constants;
using WebApi.Application.Services.Interfaces;
using WebApi.Core.Entities;
using WebApi.Core.Enums;
using WebApi.Tests.Integration.Helpers;

namespace WebApi.Tests.Integration.SignalR;

[Collection("Integration Tests")]
public class AppHubSettingsTests : BaseIntegrationTest, IAsyncLifetime
{
    public AppHubSettingsTests(IntegrationTestFixture fixture) : base(fixture) {}

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await CleanupConnectionsAsync();
        await Task.Delay(200);
    }

    private HubConnection _connection = null!;
    private string _roomHash = null!;

    private async Task SetupRoomAsync()
    {
        var db = Factory.GetDbContext();
        var room = new Room("TestRoom", "password123");
        room.RoomSettings.RoomType = RoomTypes.Private;
        db.Rooms.Add(room);
        await db.SaveChangesAsync();
        _roomHash = room.Hash!;

        var jwtService = Factory.GetRequiredService<IJwtTokenService>();
        var token = jwtService.GenerateToken("AdminUser", Role.Admin, _roomHash);

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/Hub/Room", options =>
            {
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.Headers.Add("Authorization", $"Bearer {token}");
            })
            .Build();
    }

    private async Task CleanupConnectionsAsync()
    {
        if (_connection != null)
        {
            try
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.StopAsync().ConfigureAwait(false);
                }
            }
            catch { }

            try
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
            catch { }
        }
    }

    [Fact]
    public async Task SetRoomPassword_AdminCanChangePassword_ShouldBroadcast()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();
        await _connection.StartAsync();

        string? receivedPassword = null;
        RoomTypes? receivedType = null;
        _connection.On(HubMessages.OnSetRoomPassword, (string password, RoomTypes type) =>
        {
            receivedPassword = password;
            receivedType = type;
        });

        await _connection.InvokeAsync(HubMessages.SetRoomPassword, _roomHash, "newPassword");
        await Task.Delay(100);

        receivedPassword.Should().Be("newPassword");
        receivedType.Should().Be(RoomTypes.Private);

        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.RoomSettings).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        room!.RoomSettings.RoomPassword.Should().Be("newPassword");

        await _connection.StopAsync();
    }

    [Fact]
    public async Task SetUserPermissions_AdminCanUpdatePermissions_ShouldBroadcast()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();
        await _connection.StartAsync();

        UserPermissions? receivedPermissions = null;
        _connection.On(HubMessages.OnSetUserPermissions, (UserPermissions permissions) =>
        {
            receivedPermissions = permissions;
        });

        var newPermissions = new UserPermissions
        {
            CanAddVideo = false,
            CanRemoveVideo = false,
            CanStartOrPauseVideo = true,
            CanSkipVideo = true,
            CanAddChatMessage = false
        };

        await _connection.InvokeAsync(HubMessages.SetUserPermissions, _roomHash, newPermissions);
        await Task.Delay(100);

        receivedPermissions.Should().NotBeNull();
        receivedPermissions!.CanAddVideo.Should().BeFalse();
        receivedPermissions.CanStartOrPauseVideo.Should().BeTrue();

        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.UserPermissions).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        room!.UserPermissions.CanAddVideo.Should().BeFalse();
        room.UserPermissions.CanStartOrPauseVideo.Should().BeTrue();

        await _connection.StopAsync();
    }
}
