using MyLab.Db;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Infrastructure.Database.SourceRepository;

class DbSourceRepositoryFactory(IDbManager db) : ISourceRepositoryFactory
{
    public ISourceRepository Create(IndexDescription idxDesc)
    {
        return new DbSourceRepository(idxDesc, db);
    }
}