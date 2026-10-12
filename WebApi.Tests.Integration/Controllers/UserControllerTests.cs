using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WebApi.Application.Services;
using WebApi.Api.Contracts.Output;
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

    private void SetupClient()
    {
        Factory.ClearDatabase();
        Factory.ResetSingletonState();
        _client = Factory.CreateClient();
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

        return (responseContent?.RoomHash ?? throw new InvalidOperationException("Room creation response omitted the room hash."), "");
    }

    [Fact]
    public async Task JoinRoom_ValidCredentials_ShouldSetHttpOnlyCookie()
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
        responseContent.IsAdmin.Should().BeTrue();
        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders).Should().BeTrue();
        setCookieHeaders.Should().NotBeNull();
        var setCookieHeader = setCookieHeaders!.First();
        setCookieHeader.Should().Contain("shared-watch-access-token=");
        setCookieHeader.Should().Contain("httponly");
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
        var passwordService = Factory.GetRequiredService<IRoomPasswordService>();
        var settings = db.RoomSettings.First(roomSettings => roomSettings.RoomHash == roomHash);
        settings.RoomPassword = passwordService.Hash("correctPassword");
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
        joinResponse.IsSuccessStatusCode.Should().BeTrue();
        var response = await _client.DeleteAsync($"/api/User/Leave/{roomHash}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _client?.Dispose();
    }
}
