using MyLab.Db;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Application.Data;

namespace MyLab.Search.Indexer.Infrastructure.Database.SourceRepository;

class DbSourceRepository(IndexDescription idxDesc, IDbManager db) : ISourceRepository
{
    public Task<SourceData> ReadAsync(long seed, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}