using WebApi.Api.DTO;
using WebApi.Api.HttpClasses.Output;
using WebApi.Tests.Integration.Helpers;

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

        // Act
        var response = await _client.PostAsync("/api/Room/Create", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadFromJsonAsync<RoomCreateOutput>();
        responseContent.Should().NotBeNull();
        responseContent!.RoomHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Create_DuplicateRoomName_ShouldReturnConflict()
    {
        // Arrange
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

        // Act
        var response = await _client.PostAsync("/api/Room/Create", newContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Exists_ExistingRoom_ShouldReturnOk()
    {
        // Arrange
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

        // Act
        var response = await _client.GetAsync($"/api/Room/Exists/{createContent!.RoomHash}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var _client = CreateFreshClient();

        // Act
        var response = await _client.GetAsync("/api/Room/GetAll");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var roomsResponse = await response.Content.ReadFromJsonAsync<List<RoomDTO>>();
        roomsResponse.Should().NotBeNull();
        roomsResponse!.Should().BeEmpty();
    }
}
