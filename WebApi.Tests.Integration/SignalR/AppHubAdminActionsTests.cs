using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using WebApi.Api.DTO;
using WebApi.Api.SignalR.Interfaces;
using WebApi.Application.Constants;
using WebApi.Application.Services.Interfaces;
using WebApi.Core.Entities;
using WebApi.Tests.Integration.Helpers;
using Xunit;

namespace WebApi.Tests.Integration.SignalR;

[Collection("Integration Tests")]
public class AppHubAdminActionsTests : BaseIntegrationTest, IAsyncLifetime
{
    public AppHubAdminActionsTests(IntegrationTestFixture fixture) : base(fixture) {}

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
        var room = new Room("TestRoom", "");
        var user = new User("TestUser", Role.User) { RoomHash = room.Hash };
        room.Users.Add(user);
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

        await _connection.StartAsync();
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

    /*[Fact]
    public async Task KickOutAsync_AdminCanKickUser_ShouldRemoveUserAndBroadcast()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();

        object? kickedUser = null;
        _connection.On(HubMessages.OnKickOut, (object user) =>
        {
            kickedUser = user;
        });

        await _connection.InvokeAsync(HubMessages.KickOut, _roomHash, "TestUser");
        await Task.Delay(100);

        kickedUser.Should().NotBeNull();

        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.Users).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        room!.Users.Should().BeEmpty();
    }*/

    [Fact]
    public async Task SetAdminStatusAsync_AdminCanPromoteUser_ShouldUpdateAndBroadcast()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();

        UserDTO? promotedUser = null;
        _connection.On(HubMessages.OnSetAdminStatus, (UserDTO user) =>
        {
            promotedUser = user;
        });

        await _connection.InvokeAsync(HubMessages.SetAdminStatus, _roomHash, "TestUser", true);
        await Task.Delay(100);

        promotedUser.Should().NotBeNull();
        promotedUser!.IsAdmin.Should().BeTrue();

        var db = Factory.GetDbContext();
        var user = await db.Rooms.Include(r => r.Users).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        user!.Users.First().Role.Should().Be(Role.Admin);
    }

    [Fact]
    public async Task SetAdminStatusAsync_AdminCanDemoteAdmin_ShouldUpdateAndBroadcast()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();

        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.Users).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        var adminUser = new User("AnotherAdmin", Role.Admin) { RoomHash = room!.Hash };
        room.Users.Add(adminUser);
        await db.SaveChangesAsync();
        await Task.Delay(100);

        UserDTO? demotedUser = null;
        _connection.On(HubMessages.OnSetAdminStatus, (UserDTO user) =>
        {
            demotedUser = user;
        });

        await _connection.InvokeAsync(HubMessages.SetAdminStatus, _roomHash, "AnotherAdmin", false);
        await Task.Delay(100);

        demotedUser.Should().NotBeNull();
        demotedUser!.IsAdmin.Should().BeFalse();

        var freshDb = Factory.GetDbContext();
        var updatedRoom = await freshDb.Rooms.Include(r => r.Users).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        var demotedUserInDb = updatedRoom!.Users.FirstOrDefault(u => u.Username == "AnotherAdmin");
        demotedUserInDb.Should().NotBeNull();
        demotedUserInDb!.Role.Should().Be(Role.User);
    }
}
