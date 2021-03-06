using System.IO;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Nest;
using Newtonsoft.Json.Linq;
using Xunit;
using IndexOptions = MyLab.Search.Indexer.Options.IndexOptions;

namespace UnitTests
{
    public class FileIndexResourceProviderBehavior
    {
        private readonly string _resourcePath;

        public FileIndexResourceProviderBehavior()
        {
            _resourcePath = Path.Combine(Directory.GetCurrentDirectory(), "idx-res");
        }

        [Fact]
        public async Task ShouldLoadKickQueryFromOptions()
        {
            //Arrange
            var indexerOpts = new IndexerOptions
            {
                ResourcePath = _resourcePath,
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo-index",
                        KickDbQuery = "-- kick query from options"
                    }
                }
            };

            var service = new FileIndexResourceProvider(indexerOpts);

            //Act
            var kickQuery = await service.ProvideKickQueryAsync("foo-index");

            //Assert
            Assert.Equal("-- kick query from options", kickQuery);
        }

        [Fact]
        public async Task ShouldLoadKickQueryFromFile()
        {
            //Arrange
            var indexerOpts = new IndexerOptions
            {
                ResourcePath = _resourcePath,
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo-index",
                        KickDbQuery = null
                    }
                }
            };

            var service = new FileIndexResourceProvider(indexerOpts);

            //Act
            var kickQuery = await service.ProvideKickQueryAsync("foo-index");

            //Assert
            Assert.Equal("-- kick query from file", kickQuery);
        }

        [Fact]
        public async Task ShouldLoadSyncQueryFromOptions()
        {
            //Arrange
            var indexerOpts = new IndexerOptions
            {
                ResourcePath = _resourcePath,
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo-index",
                        SyncDbQuery = "-- sync query from options"
                    }
                }
            };

            var service = new FileIndexResourceProvider(indexerOpts);

            //Act
            var syncQuery = await service.ProvideSyncQueryAsync("foo-index");

            //Assert
            Assert.Equal("-- sync query from options", syncQuery);
        }

        [Fact]
        public async Task ShouldLoadSyncQueryFromFile()
        {
            //Arrange
            var indexerOpts = new IndexerOptions
            {
                ResourcePath = _resourcePath,
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo-index",
                        SyncDbQuery = null
                    }
                }
            };

            var service = new FileIndexResourceProvider(indexerOpts);

            //Act
            var syncQuery = await service.ProvideSyncQueryAsync("foo-index");

            //Assert
            Assert.Equal("-- sync query from file", syncQuery);
        }

        [Theory]
        [InlineData("idx-res")]
        [InlineData("idx-res-2")]
        [InlineData("idx-res-3")]
        public async Task ShouldProvideIndexJson(string resDirName)
        {
            //Arrange
            var resPath = Path.Combine(Directory.GetCurrentDirectory(), resDirName);

            var indexerOpts = new IndexerOptions
            {
                ResourcePath = resPath,
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo-index"
                    }
                }
            };

            var service = new FileIndexResourceProvider(indexerOpts);

            //Act
            var indexJson = await service.ProvideIndexSettingsAsync("foo-index");
           
            var indexJObj = JObject.Parse(indexJson);

            //Assert
            Assert.Equal("long", indexJObj.SelectToken("mappings.properties.Id.type")?.Value<string>());
            Assert.Equal("text", indexJObj.SelectToken("mappings.properties.Content.type")?.Value<string>());
        }
    }
}
