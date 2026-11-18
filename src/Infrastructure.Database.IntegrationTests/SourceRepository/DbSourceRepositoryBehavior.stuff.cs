using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Db;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests.SourceRepository;

public partial class DbSourceRepositoryBehavior
{
    readonly ServiceProvider _app;
    readonly IDbManager _db;

    public DbSourceRepositoryBehavior(ITestOutputHelper output)
    {
        _app = TestTools.CreateApp(output).BuildServiceProvider();
        _db = _app.GetRequiredService<IDbManager>();
    }

    public async Task InitializeAsync()
    {
        await TestTools.ClearTestTablesAsync(_db);
    }

    public async Task DisposeAsync()
    {
        await _app.DisposeAsync();
    }
}
