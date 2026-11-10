using Microsoft.Extensions.DependencyInjection;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Application.Indexing;

class IndexWorkerFactory
    (
        IIndexDescProvider cfgProvider,
        IServiceProvider serviceProvider
    )
{
    public async Task<IReadOnlyList<IndexWorker>> CreateAllAsync(CancellationToken cancellationToken)
    {
        var configs = await cfgProvider.ProvideAllAsync(cancellationToken);

        return configs
            .Select(CreateWorker)
            .ToArray()
            .AsReadOnly();
    }

    IndexWorker CreateWorker(IndexDescription desc)
    {
        return ActivatorUtilities.CreateInstance<IndexWorker>(serviceProvider, desc);
    }
}