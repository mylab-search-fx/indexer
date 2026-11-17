using LinqToDB;
using LinqToDB.Async;
using MyLab.Db;
using MyLab.Search.Indexer.Application.Abstraction;

namespace MyLab.Search.Indexer.Infrastructure.Database.SeedStorage;

class DbSeedStorage(IDbManager db) : ISeedStorage
{
    public Task<long> GetAsync(string idxId, CancellationToken ct)
    {
        return db.DoOnce()
            .Tab<IndexSeed>()
            .Where(s => s.IndexId == idxId)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct);
    }

    public Task SetAsync(string idxId, long seed, CancellationToken ct)
    {
        return db.DoOnce()
            .Tab<IndexSeed>()
            .InsertOrUpdateAsync(
                () => new IndexSeed
                {
                    IndexId = idxId,
                    Value = seed
                },
                stored => new IndexSeed
                {
                    IndexId = idxId,
                    Value = seed
                }, ct);
    }
}