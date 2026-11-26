using Infrastructure.Database.IntegrationTests;
using Infrastructure.OpenSearch.IntegrationTests;
using Infrastructure.Redis.IntegrationTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLab.Db;
using MyLab.Log.XUnit;
using Xunit.Abstractions;

namespace Worker.E2ETests;

public partial class WorkerBehavior :
    IClassFixture<IndexerDbCleanerFixture>,
    IClassFixture<SourceDbFixture>,
    IClassFixture<SourceDb2Fixture>,
    IClassFixture<SourceDbCreatorFixture>,
    IClassFixture<SourceDb2CreatorFixture>,
    IAsyncLifetime
{
    readonly IDbManager _source1Db;
    readonly IDbManager _source2Db;
    readonly IndexerDbCleanerFixture _indexerDbCleanerFxt;
    readonly ITestOutputHelper _output;

    IServiceProvider? _app;
    DirectoryInfo? _configDir;

    public WorkerBehavior
    (
        IndexerDbCleanerFixture indexerDbCleanerFxt,
        SourceDbFixture source1Fxt,
        SourceDb2Fixture source2Fxt,
        ITestOutputHelper output
    )
    {
        indexerDbCleanerFxt.Output = output;
        source1Fxt.Output = output;
        source2Fxt.Output = output;

        _indexerDbCleanerFxt = indexerDbCleanerFxt;
        _source1Db = source1Fxt.Manager;
        _source2Db = source2Fxt.Manager;
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _configDir = Directory.CreateTempSubdirectory("mylab-indexer-test");

        await DbTestTools.CreateTestTableAsync(_source1Db);
        await DbTestTools.CreateTestTableAsync(_source2Db);

        CreateApp();
    }

    public async Task DisposeAsync()
    {
        await DbTestTools.DropTestTableAsync(_source1Db);
        await DbTestTools.DropTestTableAsync(_source2Db);
        await _indexerDbCleanerFxt.CleanupAsync();

        if(_configDir is {Exists:true})
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

    Task StartAppAsync(CancellationToken ct) => _app == null 
            ? throw new InvalidOperationException("App was not created") 
            : Task.WhenAll(_app.GetServices<IHostedService>().Select(h => h.StartAsync(ct)));

    Task StopAppAsync(CancellationToken ct) => _app == null
            ? throw new InvalidOperationException("App was not created")
            : Task.WhenAll(_app.GetServices<IHostedService>().Select(h => h.StopAsync(ct)));
}