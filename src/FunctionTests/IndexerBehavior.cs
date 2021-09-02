using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.Db;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Services;
using MyLab.TaskApp;
using Nest;
using Xunit;
using Xunit.Abstractions;

namespace FunctionTests
{
    public class IndexerBehavior : 
        IClassFixture<IndexerTestDbFixture>, 
        IClassFixture<EsFixture<TestEsConnection>>,
        IClassFixture<MqFixture>,
        IAsyncLifetime
    {
        private readonly ITestOutputHelper _output;
        private readonly MqFixture _mqFxt;
        private readonly TestApi<Startup, ITaskAppContract> _api;
        private readonly IDbManager _db;
        private readonly IEsSearcher<SearchTestEntity> _es;

        public IndexerBehavior(
            ITestOutputHelper output, 
            IndexerTestDbFixture dbFxt, 
            EsFixture<TestEsConnection> esFxt,
            MqFixture mqFxt)
        {
            _output = output;
            _mqFxt = mqFxt;

            dbFxt.Output = output;
            _db = dbFxt.Manager;

            esFxt.Output = output;
            _es = esFxt.CreateSearcher<SearchTestEntity>();

            _api = new TestApi<Startup, ITaskAppContract>
            {
                Output = output
            };
        }

        [Fact]
        public async Task ShouldInterpretNullAsAbsentProperty()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var testEntity = new TestEntity
            {
                Id = 2,
                Value = "foo",
                Bool = null
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(testEntity.Id)));

            var client = _api.StartWithProxy(srv =>
            {
                srv.Configure<IndexerDbOptions>(o =>
                {
                    o.Provider = "mysql";
                }
                );

                srv.Configure<IndexerOptions>(o =>
                {
                    o.Jobs = new[]
                    {
                            new JobOptions
                            {
                                JobId = "foojob",

                                DbQuery = "select * from test",
                                NewUpdatesStrategy = NewUpdatesStrategy.Add,
                                NewIndexStrategy = NewIndexStrategy.File,
                                IdProperty = nameof(TestEntity.Id),
                                EsIndex = indexName
                            }
                        };
                });

                srv.Configure<ElasticsearchOptions>(o =>
                {
                    o.Url = "http://localhost:9200";
                }
                );

                srv.AddSingleton<IConnectionStringProvider, TestDbCsProvider>();
                srv.AddSingleton<ISeedService, TestSeedService>();

                srv.AddSingleton<IJobResourceProvider>(new TestJobResourceProvider("test-entity-map.json"));

                srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
            }
            );

            await _db.DoOnce().InsertAsync(testEntity);

            //Act

            await client.PostProcessAsync();
            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(testEntity.Value, searchRes.First().Value);
            Assert.Null(searchRes.First().Bool);
        }

        [Fact]
        public async Task ShouldUseBoolRight()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var testEntity = new TestEntity
            {
                Id = 2,
                Value = "foo",
                Bool = true
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(testEntity.Id)));

            var client = _api.StartWithProxy(srv =>
            {
                srv.Configure<IndexerDbOptions>(o =>
                    {
                        o.Provider = "mysql";
                    }
                );

                srv.Configure<IndexerOptions>(o =>
                {
                    o.Jobs = new[]
                    {
                            new JobOptions
                            {
                                JobId = "foojob",

                                DbQuery = "select * from test",
                                NewUpdatesStrategy = NewUpdatesStrategy.Add,
                                NewIndexStrategy = NewIndexStrategy.File,
                                IdProperty = nameof(TestEntity.Id),
                                EsIndex = indexName
                            }
                        };
                });

                srv.Configure<ElasticsearchOptions>(o =>
                {
                    o.Url = "http://localhost:9200";
                }
                );

                srv.AddSingleton<IConnectionStringProvider, TestDbCsProvider>();
                srv.AddSingleton<ISeedService, TestSeedService>();

                srv.AddSingleton<IJobResourceProvider>(new TestJobResourceProvider("test-entity-map.json"));

                srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
            }
            );

            await _db.DoOnce().InsertAsync(testEntity);

            //Act

            await client.PostProcessAsync();
            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(testEntity.Value, searchRes.First().Value);
            Assert.True(searchRes.First().Bool);
        }

        [Fact]
        public async Task ShouldIndexFromDb()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");
            
            var testEntity = new TestEntity
            {
                Id = 2,
                Value = "foo"
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(testEntity.Id)));

            var client = _api.StartWithProxy(srv =>
                {
                    srv.Configure<IndexerDbOptions>(o =>
                        {
                            o.Provider = "mysql";
                            
                        }
                    );

                    srv.Configure<IndexerOptions>(o =>
                    {
                        o.Jobs = new[]
                        {
                            new JobOptions
                            {
                                JobId = "foojob",

                                DbQuery = "select * from test",
                                NewUpdatesStrategy = NewUpdatesStrategy.Add,
                                NewIndexStrategy = NewIndexStrategy.Auto,
                                IdProperty = nameof(TestEntity.Id),
                                EsIndex = indexName
                            }
                        };
                    });

                    srv.Configure<ElasticsearchOptions>(o =>
                        {
                            o.Url = "http://localhost:9200";
                        }
                    );

                    srv.AddSingleton<IConnectionStringProvider, TestDbCsProvider>();

                    srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
                }
            );

            await _db.DoOnce().InsertAsync(testEntity);
            
            //Act

            await client.PostProcessAsync();
            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);            

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(testEntity.Value, searchRes.First().Value);
        }

        [Fact]
        public async Task ShouldIndexFromMq()
        {
            //Arrange
            var queue = _mqFxt.CreateWithRandomId();
            _output.WriteLine("MqQueue created: " + queue.Name);

            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var testEntity = new SearchTestEntity
            {
                Id = 2,
                Value = "foo",
                Bool = false
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(testEntity.Id)));

            _api.StartWithProxy(srv =>
                {

                    srv.Configure<IndexerOptions>(o =>
                        {
                            o.Jobs = new JobOptions[]
                            {
                                new JobOptions
                                {
                                    JobId = "foojob",
                                    MqQueue = queue.Name,
                                    NewIndexStrategy = NewIndexStrategy.Auto,
                                    IdProperty = nameof(TestEntity.Id),
                                    EsIndex = indexName
                                },

                            };
                        }
                    );

                    srv.Configure<ElasticsearchOptions>(o =>
                        {
                            o.Url = "http://localhost:9200";
                        }
                    );

                    srv.ConfigureRabbitClient(o =>
                        {
                            o.Host = "localhost";
                            o.Port = 5672;
                            o.Password = "guest";
                            o.User = "guest";
                        }
                    );

                    srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
                }
            );

            //Act
            queue.Publish(testEntity);

            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(testEntity.Value, searchRes.First().Value);
        }

        [Fact]
        public async Task ShouldOverrideMqFromDb()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var queue = _mqFxt.CreateWithRandomId();
            _output.WriteLine("MqQueue created: " + queue.Name);

            var initialTestEntity = new TestEntity
            {
                Id = 2,
                Value = "foo"
                
            };

            var overrideTestEntity = new TestEntity
            {
                Id = 2,
                Value = "bar"
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(initialTestEntity.Id)));

            var client = _api.StartWithProxy(srv =>
                {
                    srv.Configure<IndexerDbOptions>(o =>
                        {
                            o.Provider = "mysql";
                        }
                    );

                    srv.Configure<IndexerOptions>(o =>
                        {
                            o.Jobs = new[]
                            {
                                new JobOptions
                                {
                                    JobId = "foojob",
                                    NewIndexStrategy = NewIndexStrategy.Auto,
                                    IdProperty = nameof(TestEntity.Id),
                                    MqQueue = queue.Name,
                                    DbQuery = "select * from test",
                                    NewUpdatesStrategy = NewUpdatesStrategy.Add,
                                    EsIndex = indexName
                                }
                            };
                        }
                    );

                    srv.Configure<ElasticsearchOptions>(o =>
                        {
                            o.Url = "http://localhost:9200";
                        }
                    );

                    srv.ConfigureRabbitClient(o =>
                    {
                        o.Host = "localhost";
                        o.Port = 5672;
                        o.Password = "guest";
                        o.User = "guest";
                    });

                    srv.AddSingleton<IConnectionStringProvider, TestDbCsProvider>();

                    srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
                }
            );

            queue.Publish(initialTestEntity);
            await Task.Delay(500);

            await _db.DoOnce().InsertAsync(overrideTestEntity);

            //Act


            await client.PostProcessAsync();
            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);            

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(overrideTestEntity.Value, searchRes.First().Value);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _api.Dispose();
            return Task.CompletedTask;
        }

        public class SearchTestEntity
        {
            [Number(Name = "Id")]
            public long Id { get; set; }
            [Text(Name = "Value")]
            public string Value { get; set; }

            [Boolean(Name = "Bool")]
            public bool? Bool { get; set; }
        }

        class TestJobResourceProvider : IJobResourceProvider
        {
            private readonly string _filepath;

            public TestJobResourceProvider(string filepath)
            {
                _filepath = filepath;
            }

            public Task<string> ReadFileAsync(string jobId, string filename)
            {
                return File.ReadAllTextAsync(Path.Combine("files", _filepath));
            }
        }

        class TestSeedService : ISeedService
        {
            private string _seed;

            public Task WriteAsync(string jobId, string seed)
            {
                _seed = seed;

                return Task.CompletedTask;
            }

            public Task<string> ReadAsync(string jobId)
            {
                return Task.FromResult(_seed);
            }
        }
    }
}