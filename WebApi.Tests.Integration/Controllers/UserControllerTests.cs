using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WebApi.Api.HttpClasses.Output;
using WebApi.Application.Services.Interfaces;
using WebApi.Tests.Integration.Helpers;
using Xunit;

namespace WebApi.Tests.Integration.Controllers;

[Collection("Integration Tests")]
public class UserControllerTests : BaseIntegrationTest
{
    public UserControllerTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    private HttpClient _client = null!;
    private IJwtTokenService _tokenService = null!;

    private void SetupClient()
    {
        Factory.ClearDatabase();
        Factory.ResetSingletonState();
        _client = Factory.CreateClient();
        _tokenService = Factory.GetRequiredService<IJwtTokenService>();
    }

    private async Task<(string roomHash, string roomPassword)> CreateTestRoomAsync(string roomName)
    {
        var input = new
        {
            roomName = roomName,
            roomPassword = "",
            username = "TestUser"
        };
        var content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Room/Create", content);
        var responseContent = await response.Content.ReadFromJsonAsync<RoomCreateOutput>();

        return (responseContent!.RoomHash, "");
    }

    private void AddAuthorizationHeader(string token)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    [Fact]
    public async Task JoinRoom_ValidCredentials_ShouldReturnOkWithToken()
    {
        Factory.ResetSingletonState();
        SetupClient();
        var (roomHash, roomPassword) = await CreateTestRoomAsync("TestRoom");

        var input = new
        {
            username = "TestUser",
            roomPassword = roomPassword
        };
        var content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"/api/User/Join/{roomHash}", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadFromJsonAsync<RoomJoinOutput>();
        responseContent.Should().NotBeNull();
        responseContent!.AuthorizationToken.Should().NotBeNullOrEmpty();
        responseContent.IsAdmin.Should().BeTrue();
        _client?.Dispose();
    }

    [Fact]
    public async Task JoinRoom_InvalidRoom_ShouldReturnNotFound()
    {
        Factory.ResetSingletonState();
        SetupClient();

        var input = new
        {
            username = "TestUser",
            roomPassword = ""
        };
        var content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/User/Join/nonexistent", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _client?.Dispose();
    }

    [Fact]
    public async Task JoinRoom_WrongPassword_ShouldReturnUnauthorized()
    {
        Factory.ResetSingletonState();
        SetupClient();
        var (roomHash, _) = await CreateTestRoomAsync("PrivateRoom");

        var db = Factory.GetDbContext();
        var room = await db.Rooms.FindAsync(roomHash);
        room!.RoomSettings.RoomPassword = "correctPassword";
        await db.SaveChangesAsync();
        await Task.Delay(100);

        var input = new
        {
            username = "TestUser",
            roomPassword = "wrongPassword"
        };
        var content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"/api/User/Join/{roomHash}", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        _client?.Dispose();
    }

    [Fact]
    public async Task JoinRoom_DuplicateUsername_ShouldReturnConflict()
    {
        Factory.ResetSingletonState();
        SetupClient();
        var (roomHash, roomPassword) = await CreateTestRoomAsync("TestRoom");

        var input = new
        {
            username = "TestUser",
            roomPassword = roomPassword
        };
        var content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");

        await _client.PostAsync($"/api/User/Join/{roomHash}", content);

        var newContent = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"/api/User/Join/{roomHash}", newContent);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _client?.Dispose();
    }

    [Fact]
    public async Task LeaveRoom_ValidUser_ShouldReturnOk()
    {
        Factory.ResetSingletonState();
        SetupClient();
        var (roomHash, roomPassword) = await CreateTestRoomAsync("TestRoom");

        var input = new
        {
            username = "TestUser",
            roomPassword = roomPassword
        };
        var content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");

        var joinResponse = await _client.PostAsync($"/api/User/Join/{roomHash}", content);
        var joinContent = await joinResponse.Content.ReadFromJsonAsync<RoomJoinOutput>();

        AddAuthorizationHeader(joinContent!.AuthorizationToken);
        var response = await _client.DeleteAsync($"/api/User/Leave/{roomHash}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _client?.Dispose();
    }
}
