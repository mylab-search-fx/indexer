using Infrastructure.Database.IntegrationTests;
using Xunit.Abstractions;

namespace Worker.IntegrationTests;

public class WorkerBehavior: 
    IClassFixture<IndexerDbFixture>,
    IClassFixture<LocalServerDbFixture>,
    IAsyncLifetime
{
    
    public WorkerBehavior(IndexerDbFixture indexerFxt, LocalServerDbFixture serverFxt, ITestOutputHelper output)
    {
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}