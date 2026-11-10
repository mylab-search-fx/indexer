using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Infrastructure.DB;

class DBSourceRepositoryFactory : ISourceRepositoryFactory
{
    public ISourceRepository Create(IndexDescription idxDesc)
    {
        throw new NotImplementedException();
    }
}