namespace WebApi.Tests.Integration.Helpers;

[CollectionDefinition("Integration Tests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}

public class IntegrationTestFixture : IAsyncLifetime
{
    public CustomWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new CustomWebApplicationFactory();
        await Factory.InitializeAsync();
        Factory.ClearDatabase();
    }

    public Task DisposeAsync()
    {
        Factory.ClearDatabase();
        return Task.CompletedTask;
    }
}

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestFixture>
{
    protected CustomWebApplicationFactory Factory { get; }

    protected BaseIntegrationTest(IntegrationTestFixture fixture)
    {
        Factory = fixture.Factory;
    }
}
