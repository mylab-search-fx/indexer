using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Application.Data;

namespace MyLab.Search.Indexer.Infrastructure.Opensearch;

class OpensearchDestinationRepositoryFactory : IDestinationRepositoryFactory
{
    public IDestinationRepository Create(IndexDescription idxDesc)
    {
        throw new NotImplementedException();
    }
}

class OpensearchDestinationRepository : IDestinationRepository
{
    public Task WriteAsync(SourceData data)
    {
        throw new NotImplementedException();
    }
}