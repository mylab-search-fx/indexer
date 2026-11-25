using LinqToDB.Data;
using MyLab.Db;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests.SourceRepository;

public partial class DbSourceRepositoryBehavior : 
    IClassFixture<LocalServerDbFixture>, 
    IClassFixture<SourceDbFixture>,
    IAsyncLifetime
{
    const int PageSize = 2;
    readonly IDbManager _srvDb;
    readonly IDbManager _srcDb;
    TestDbItem[] _items;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public DbSourceRepositoryBehavior
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    (
            LocalServerDbFixture srvFxt, 
            SourceDbFixture srcFxt, 
            ITestOutputHelper output
        )
    {
        srvFxt.Output = output;
        srcFxt.Output = output;

        _srvDb = srvFxt.Manager;
        _srcDb = srcFxt.Manager;
    }

    public async Task InitializeAsync()
    {
        await using var c = _srvDb.Use();

        await DbTestTools.CreateTestDbAsync(_srvDb);
        await DbTestTools.CreateTestTableAsync(_srcDb);

        await FillDbAsync();
    }

    async Task FillDbAsync()
    {
        _items = Enumerable.Range(0, 4)
            .Select(i => new TestDbItem
            {
                Id = i,
                Value = "value-" +i,
                LastModified = DateTime.Now.AddSeconds(i)
            })
            .ToArray();

        await _srcDb.DoOnce().BulkCopyAsync(_items);
    }

    public Task DisposeAsync() => DbTestTools.DropTestDbAsync(_srvDb);

    static long DtToLongAndRound(DateTime dateTime)
        => dateTime.Ticks / 10_000_000 * 10_000_000;
}