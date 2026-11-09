using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Application.Indexing
{
    internal class IndexWorker(IndexConfig cfg) : IIndexWorker
    {
        public string Name { get; } = cfg.Name;
        
        public Task RunAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
