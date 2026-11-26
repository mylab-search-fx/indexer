using LinqToDB.Data;
using MyLab.Db;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests.SourceRepository;

public partial class DbSourceRepositoryBehavior : 
    IClassFixture<LocalServerDbFixture>, 
    IClassFixture<SourceDbFixture>,
    IClassFixture<SourceDbCreatorFixture>,
    IAsyncLifetime
{
    const int PageSize = 2;
    readonly IDbManager _srcDb;
    TestDbItem[] _items;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public DbSourceRepositoryBehavior
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    (
            SourceDbFixture srcFxt, 
            ITestOutputHelper output
        )
    {
        srcFxt.Output = output;
        _srcDb = srcFxt.Manager;
    }

    public async Task InitializeAsync()
    {
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

    public Task DisposeAsync()
    {
        return DbTestTools.DropTestTableAsync(_srcDb);
    }

    static long DtToLongAndRound(DateTime dateTime)
        => dateTime.Ticks / 10_000_000 * 10_000_000;
}