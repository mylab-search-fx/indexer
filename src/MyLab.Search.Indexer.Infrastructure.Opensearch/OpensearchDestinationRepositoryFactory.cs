using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Infrastructure.Opensearch;

class OpensearchDestinationRepositoryFactory : IDestinationRepositoryFactory
{
    public IDestinationRepository Create(IndexDescription idxDesc)
    {
        throw new NotImplementedException();
    }
}