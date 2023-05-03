﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.Log.XUnit;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Services.ResourceUploading;
using MyLab.Search.Indexer.Tools;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class IndexTemplateUploaderBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>, IAsyncLifetime
    {
        private readonly EsFixture<TestEsFixtureStrategy> _fxt;
        private readonly ITestOutputHelper _output;
        private readonly string _indexerVer;

        public IndexTemplateUploaderBehavior(EsFixture<TestEsFixtureStrategy> fxt, ITestOutputHelper output)
        {
            _fxt = fxt;
            _output = output;
            fxt.Output = output;

            _indexerVer = typeof(IResourceUploader).Assembly.GetName().Version?.ToString();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _fxt.Tools.IndexTemplate("index-template-test").DeleteAsync();
        }

        [Fact]
        public async Task ShouldUploadIfDoesNotExists()
        {
            //Arrange
            var idxResProviderMock = new Mock<IResourceProvider>();
            
            var templateResource = new TestResource("index-template-test", "resources\\index-template-example.json");

            idxResProviderMock.Setup(p => p.ProvideIndexTemplates())
                .Returns(() => new IResource[] { templateResource });
            
            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )            
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxResProviderMock.Object)
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<IndexTemplateUploader>(services);

            ServiceMetadata metadata = null;

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                metadata = ServiceMetadata.Extract(templateInfo.Meta);
            }

            //Assert
            Assert.NotNull(metadata);
            Assert.Equal(ServiceMetadata.MyCreator, metadata.Creator);
            Assert.Equal("1", metadata.Ver);
            Assert.NotNull(metadata.History);
            Assert.Single(metadata.History);
            Assert.Equal(_indexerVer, metadata.History[0].ActorVer);
            Assert.Equal(ServiceMetadata.MyCreator, metadata.History[0].Actor);
            Assert.Equal(DateTime.Now.Date, metadata.History[0].ActDt.Date);
            Assert.Equal("1", metadata.History[0].ComponentVer);
        }

        [Fact]
        public async Task ShouldUpdateIfExists()
        {
            //Arrange
            var idxResProviderMock = new Mock<IResourceProvider>();
            
            var templateResource = new TestResource("index-template-test", "resources\\index-template-example-2.json");

            idxResProviderMock.Setup(p => p.ProvideIndexTemplates())
                .Returns(() => new IResource[] { templateResource });
            
            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxResProviderMock.Object)
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<IndexTemplateUploader>(services);

            ServiceMetadata metadata = null;

            var templateJson = await File.ReadAllTextAsync("resources\\existent-index-template.json");
            await _fxt.Tools.IndexTemplate("index-template-test").PutAsync(templateJson);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                metadata = ServiceMetadata.Extract(templateInfo.Meta);
            }

            //Assert
            Assert.NotNull(metadata);
            Assert.Equal(ServiceMetadata.MyCreator, metadata.Creator);
            Assert.Equal("2", metadata.Ver);
            Assert.NotNull(metadata.History);
            Assert.Equal(2, metadata.History.Length);

            Assert.Equal("1.0.0", metadata.History[0].ActorVer);
            Assert.Equal(ServiceMetadata.MyCreator, metadata.History[0].Actor);
            Assert.Equal(new DateTime(2023, 01, 01, 01,02,03), metadata.History[0].ActDt);
            Assert.Equal("1", metadata.History[0].ComponentVer);

            Assert.Equal(_indexerVer, metadata.History[1].ActorVer);
            Assert.Equal(ServiceMetadata.MyCreator, metadata.History[1].Actor);
            Assert.Equal(DateTime.Now.Date, metadata.History[1].ActDt.Date);
            Assert.Equal("2", metadata.History[1].ComponentVer);
        }

        [Fact]
        public async Task ShouldNotUpdateWithSameVersion()
        {
            //Arrange
            var idxResProviderMock = new Mock<IResourceProvider>();

            var templateResource = new TestResource("index-template-test", "resources\\index-template-example.json");

            idxResProviderMock.Setup(p => p.ProvideIndexTemplates())
                .Returns(() => new IResource[] { templateResource });

            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxResProviderMock.Object)
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<IndexTemplateUploader>(services);

            ServiceMetadata metadata = null;

            var templateJson = await File.ReadAllTextAsync("resources\\existent-index-template.json");
            await _fxt.Tools.IndexTemplate("index-template-test").PutAsync(templateJson);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                metadata = ServiceMetadata.Extract(templateInfo.Meta);
            }

            //Assert
            Assert.NotNull(metadata);
            Assert.Equal(ServiceMetadata.MyCreator, metadata.Creator);
            Assert.Equal("1", metadata.Ver);
            Assert.NotNull(metadata.History);
            Assert.Single(metadata.History);
            Assert.Equal("1.0.0", metadata.History[0].ActorVer);
            Assert.Equal(ServiceMetadata.MyCreator, metadata.History[0].Actor);
            Assert.Equal(new DateTime(2023, 01, 01, 01, 02, 03), metadata.History[0].ActDt);
            Assert.Equal("1", metadata.History[0].ComponentVer);
        }

        class TestResource : IResource
        {
            private readonly string _filename;
            public string Name { get; }
            public Stream OpenRead()
            {
                return File.OpenRead(_filename);
            }

            public TestResource(string name, string filename)
            {
                Name = name;
                _filename = filename;
            }
        }
    }
}