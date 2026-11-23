using LinqToDB;
using MyLab.Db;
using MyLab.DbTest;
using Xunit.Abstractions;

namespace Worker.IntegrationTests;

public class WorkerBehavior: IClassFixture<RemoteDbFixture>, IAsyncLifetime
{
    IDbManager db;

    public WorkerBehavior(RemoteDbFixture fxt, ITestOutputHelper output)
    {
        fxt.Output = output;
        db = fxt.Manager;
    }

    public Task InitializeAsync()
    {
        
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}