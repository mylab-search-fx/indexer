using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Log.XUnit;
using MyLab.RabbitClient.Publishing;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class IncomingRequestMqProcessingBehavior
    {
        private readonly ITestOutputHelper _output;

        public IncomingRequestMqProcessingBehavior(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ShouldGetRequestThroughQueue()
        {
            //Arrange
            var postWithoutIdEnt = new TestDoc(){Content = Guid.NewGuid().ToString("N")};
            var postEnt = TestDoc.Generate();
            var putEnt = TestDoc.Generate();
            var patchEnt = TestDoc.Generate();

            var deleteId = Guid.NewGuid().ToString("N");
                
            var testReq = new MyLab.Search.IndexerClient.IndexingMqMessage
            {
                IndexId = "foo-index",
                Put = new []
                {
                    JObject.FromObject(putEnt) 
                },
                Patch = new[]
                {
                    JObject.FromObject(patchEnt)
                },
                Delete = new []
                {
                    deleteId
                }
            };

            var config = new ConfigurationBuilder()
                .Build();

            var srvCollection = new ServiceCollection();

            var startup = new Startup(config);
            startup.ConfigureServices(srvCollection);
            srvCollection.AddRabbitEmulation();

            srvCollection.AddLogging(l => l.AddFilter(lvl => true).AddXUnit(_output));

            var inputSrvProc = new TestInputRequestProcessor();
            srvCollection.AddSingleton<IInputRequestProcessor>(inputSrvProc);

            srvCollection.Configure<IndexerOptions>(opt => opt.MqQueue = "foo-queue");

            var serviceProvider = srvCollection.BuildServiceProvider();
            var publisher = serviceProvider.GetRequiredService<IRabbitPublisher>();

            //Act
            publisher
                .IntoQueue("foo-queue")
                .SetJsonContent(testReq)
                .Publish();

            var actualRequest = inputSrvProc.LastRequest;
            
            var actualPutIndexEnt = actualRequest.PutList?.FirstOrDefault(e => e.GetIdProperty() == putEnt.Id);
            var actualPatchIndexEnt = actualRequest.PatchList?.FirstOrDefault(e => e.GetIdProperty() == patchEnt.Id);
            
            var actualPutEnt = actualPutIndexEnt?.ToObject<TestDoc>();
            var actualPatchEnt = actualPatchIndexEnt?.ToObject<TestDoc>();

            //Assert
            Assert.Equal("foo-index", actualRequest.IndexId);

            Assert.NotNull(actualPutIndexEnt);
            Assert.Equal(putEnt.Id, actualPutIndexEnt.GetIdProperty());
            Assert.NotNull(actualPutEnt);
            Assert.Equal(putEnt.Id, actualPutEnt.Id);
            Assert.Equal(putEnt.Content, actualPutEnt.Content);

            Assert.NotNull(actualPatchIndexEnt);
            Assert.Equal(patchEnt.Id, actualPatchIndexEnt.GetIdProperty());
            Assert.NotNull(actualPatchEnt);
            Assert.Equal(patchEnt.Id, actualPatchEnt.Id);
            Assert.Equal(patchEnt.Content, actualPatchEnt.Content);

            Assert.Single(actualRequest.DeleteList);
            Assert.Equal(deleteId, actualRequest.DeleteList[0]);
        }

        
    }
}