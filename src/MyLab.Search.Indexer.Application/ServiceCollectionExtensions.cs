using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Indexing;
using MyLab.Search.Indexer.Application.Options;

namespace MyLab.Search.Indexer.Application;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection srv)
    {
        public IServiceCollection AddIndexerAppLogic()
            => srv.AddSingleton<IIndexWorkerOrchestrator, IIndexWorkerOrchestrator>()
                .AddSingleton<IndexWorkerFactory>();

        public IServiceCollection ConfigureIndexerAppLogic(IConfiguration cfg)
            => srv.Configure<WorkerOptions>(cfg.GetSection(WorkerOptions.SectionName));
    }
}