using MyLab.Search.Indexer.Application.Abstraction;

namespace MyLab.Search.Indexer.Application.Indexing
{
    public class IndexWorkerFactory(IIndexConfigProvider cfgProvider) : IIndexWorkerFactory
    {
        public async Task<IReadOnlyList<IIndexWorker>> CreateAllAsync(CancellationToken cancellationToken)
        {
            var configs = await cfgProvider.ProvideAllAsync(cancellationToken);

            return configs
                .Select(c => new IndexWorker(c))
                .ToArray()
                .AsReadOnly();
        }
    }
}
