using Microsoft.Extensions.Configuration;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Indexing;
using MyLab.Search.Indexer.Application.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection srv)
    {
        public IServiceCollection AddIndexerAppLogic()
            => srv.AddSingleton<IIndexWorkerOrchestrator, IIndexWorkerOrchestrator>()
                .AddSingleton<IndexWorkerFactory>();

        public IServiceCollection ConfigureIndexerAppLogic(IConfiguration cfg)
            => srv.Configure<WorkerOptions>(cfg.GetSection(WorkerOptions.SectionName));

        public IServiceCollection ConfigureIndexerAppLogic(Action<WorkerOptions> configure)
            => srv.Configure<WorkerOptions>(configure);
    }
}