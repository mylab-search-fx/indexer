using Microsoft.Extensions.DependencyInjection;
using MyLab.Db;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests.SourceRepository;

public partial class SourceDataItemReaderBehavior
{
    readonly ServiceProvider _app;
    readonly IDbManager _db;

    public SourceDataItemReaderBehavior(ITestOutputHelper output)
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
