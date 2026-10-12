using WebApi.Application.DTO;
using WebApi.Tests.Integration.Helpers;
using WebApi.Api.Contracts.Output;

namespace WebApi.Tests.Integration.Controllers;

[Collection("Integration Tests")]
public class RoomControllerTests : IClassFixture<IntegrationTestFixture>
{
    private readonly CustomWebApplicationFactory _factory;

    public RoomControllerTests(IntegrationTestFixture fixture)
    {
        _factory = fixture.Factory;
    }

    private HttpClient CreateFreshClient()
    {
        _factory.ClearDatabase();
        _factory.ResetSingletonState();
        return _factory.CreateClient();
    }

    [Fact]
    public async Task Create_ValidRoom_ShouldReturnCreatedStatusCode()
    {
        using var _client = CreateFreshClient();
        var input = new
        {
            roomName = "TestRoom",
            roomPassword = "",
            username = "TestUser"
        };
        var content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Room/Create", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadFromJsonAsync<RoomCreateOutput>();
        responseContent.Should().NotBeNull();
        responseContent!.RoomHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Create_DuplicateRoomName_ShouldReturnConflict()
    {
        using var _client = CreateFreshClient();
        var input = new
        {
            roomName = "TestRoom",
            roomPassword = "",
            username = "TestUser"
        };
        var content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");

        await _client.PostAsync("/api/Room/Create", content);

        var newContent = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Room/Create", newContent);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Exists_ExistingRoom_ShouldReturnOk()
    {
        using var _client = CreateFreshClient();
        var input = new
        {
            roomName = "TestRoom",
            roomPassword = "",
            username = "TestUser"
        };
        var content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");

        var createResponse = await _client.PostAsync("/api/Room/Create", content);
        var createContent = await createResponse.Content.ReadFromJsonAsync<RoomCreateOutput>();

        var response = await _client.GetAsync($"/api/Room/Exists/{createContent!.RoomHash}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_EmptyDatabase_ShouldReturnEmptyList()
    {
        using var _client = CreateFreshClient();

        var response = await _client.GetAsync("/api/Room/GetAll");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var roomsResponse = await response.Content.ReadFromJsonAsync<List<RoomDTO>>();
        roomsResponse.Should().NotBeNull();
        roomsResponse!.Should().BeEmpty();
    }
}
