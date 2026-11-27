using Infrastructure.Database.IntegrationTests;
using Infrastructure.OpenSearch.IntegrationTests;
using Infrastructure.Redis.IntegrationTests;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLab.Db;
using MyLab.Log.XUnit;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Infrastructure.FileSystem;
using OpenSearch.Client;
using Xunit.Abstractions;

namespace Worker.E2ETests;

public partial class WorkerBehavior :
    IClassFixture<IndexerDbCleanerFixture>,
    IClassFixture<SourceDbFixture>,
    IClassFixture<SourceDb2Fixture>,
    IClassFixture<SourceDbCreatorFixture>,
    IClassFixture<SourceDb2CreatorFixture>,
    IClassFixture<TestOpenSearchIndexFixture<TestOpenSearchItem>>,
    IAsyncLifetime
{
    readonly IDbManager _source1Db;
    readonly IDbManager _source2Db;
    readonly IndexerDbCleanerFixture _indexerDbCleanerFxt;
    readonly ITestOutputHelper _output;

    IServiceProvider? _app;
    DirectoryInfo? _configDir;
    readonly TestOpenSearchIndexFixture<TestOpenSearchItem> _opensearchFxt;

    public WorkerBehavior
    (
        IndexerDbCleanerFixture indexerDbCleanerFxt,
        SourceDbFixture source1Fxt,
        SourceDb2Fixture source2Fxt,
        TestOpenSearchIndexFixture<TestOpenSearchItem> opensearchFxt,
        ITestOutputHelper output
    )
    {
        //indexerDbCleanerFxt.Output = output;
        //source1Fxt.Output = output;
        //source2Fxt.Output = output;

        _opensearchFxt = opensearchFxt;
        _indexerDbCleanerFxt = indexerDbCleanerFxt;
        _source1Db = source1Fxt.Manager;
        _source2Db = source2Fxt.Manager;
        _output = output;
    }

    public async Task InitializeAsync()
    {
        await WriteConfigsAsync();

        await DbTestTools.CreateOrResetTestTableAsync(_source1Db);
        await DbTestTools.CreateOrResetTestTableAsync(_source2Db);

        await _opensearchFxt.CleanupAsync();

        CreateApp();
    }

    public async Task DisposeAsync()
    {
        await DbTestTools.DropTestTableAsync(_source1Db);
        await DbTestTools.DropTestTableAsync(_source2Db);
        await _indexerDbCleanerFxt.CleanupAsync();
        
        await _opensearchFxt.CleanupAsync();

        if (_configDir is {Exists:true})
            _configDir.Delete(true);
    }

    void CreateApp()
    {
        var dbConfig = new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string?>
            {
                { "DB", DbTestTools.CreateCs() }
            }
        };

        var cfg = new ConfigurationBuilder()
            .Add(dbConfig)
            .Build();

        _app = new ServiceCollection()
            .AddLogging(b => b.AddXUnit(_output))
            .AddWorker()
            .ConfigureWorker(cfg)
            .ConfigureIndexerAppLogic(o =>
            {
                // nothing
            })
            .ConfigureFileSystemInfrastructure(o =>
            {
                o.ConfigPath = _configDir!.FullName;
            })
            .ConfigureOpenSearchInfrastructure(o =>
            {
                // nothing
            },
            OpenSearchTestTools.ConfigureLocalOpenSearch)
            .ConfigureRedisInfrastructure(RedisTestTools.ConfigureLocalRedis)
            .BuildServiceProvider();
    }

    async Task WriteConfigsAsync()
    {
        _configDir = Directory.CreateTempSubdirectory("mylab-indexer-test");

        var indexConfig = new IndexConfig(PageSize: 10, IdleSec: 1);

        await IndexDescriptionFileTool.WriteAllIndexDescAsync
        (
            [
                DbTestTools.CreateIntSeedIndex(indexConfig),
                DbTestTools.CreateDtSeedIndex(indexConfig)
            ],
            _configDir.FullName
        );
    }

    async Task<IAsyncDisposable> StartAppAsync()
    {
        if (_app == null) throw new InvalidOperationException("App was not created");
        
        await Task.WhenAll(_app.GetServices<IHostedService>().Select(h => h.StartAsync(CancellationToken.None)));

        return new AppStopper(_app);
    }

    async Task<IReadOnlyList<TestDbItem>> WriteItemsAsync(int shift, int cound)
    {
        var items = Enumerable.Range(shift, cound)
            .Select(i => new TestDbItem
            {
                Id = i,
                Value = "value-" + i,
                LastModified = DateTime.Now.AddSeconds(i)
            })
            .ToArray()
            .AsReadOnly();

        await _source1Db.DoOnce().BulkCopyAsync(items);
        await _source2Db.DoOnce().BulkCopyAsync(items);

        return items;
    }

    async Task<IReadOnlyCollection<TestOpenSearchItem>> ReadIndexedItemsAsync(IEnumerable<TestDbItem> items)
    {
        if (_app == null) throw new InvalidOperationException("App was not created");

        var opensearch = _app.GetRequiredService<IOpenSearchClient>();

        if (_opensearchFxt.IndexName == null) 
            throw new InvalidOperationException("OpenSearch index is not ready");

        var response = await opensearch.SearchAsync<TestOpenSearchItem>(s => s
            .Index(_opensearchFxt.IndexName)
            .Size(1000)
            .Query(q => q.MatchAll())
        );

        return response.Documents;
    }

    int FoundCount(IEnumerable<TestDbItem> dbItems, IEnumerable<TestOpenSearchItem> indexedItems)
        => dbItems.Count(dbItm => indexedItems.Any(indexedItm =>
            dbItm.Id == indexedItm.Id &&
            dbItm.Value == indexedItm.Value &&
            dbItm.LastModified == indexedItm.LastModified));

    Task WaitingForIndexer() => Task.Delay(1000);

    class AppStopper : IAsyncDisposable
    {
        readonly IServiceProvider _serviceProvider;

        public AppStopper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_serviceProvider.GetServices<IHostedService>().Select(h => h.StopAsync(CancellationToken.None)));
        }
    }
}