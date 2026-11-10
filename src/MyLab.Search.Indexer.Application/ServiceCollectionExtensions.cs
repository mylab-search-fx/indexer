using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Indexing;
using MyLab.Search.Indexer.Application.Options;

namespace MyLab.Search.Indexer.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIndexerAppLogic(this IServiceCollection srv, IConfiguration cfg)
        {
            return srv.AddSingleton<IIndexWorkerOrchestrator, IIndexWorkerOrchestrator>()
                .AddSingleton<IndexWorkerFactory>()
                .Configure<WorkerOptions>(cfg.GetSection(WorkerOptions.SectionName));
        }
    }
}
