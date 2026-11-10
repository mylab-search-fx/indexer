using MyLab.Search.Indexer.Application.Abstraction;

namespace MyLab.Search.Indexer.Infrastructure.DB;

class DBSeedStorage : ISeedStorage
{
    public Task<long> GetAsync(string idxId)
    {
        throw new NotImplementedException();
    }

    public Task SetAsync(string idxId, long seed)
    {
        throw new NotImplementedException();
    }
}