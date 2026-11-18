using MyLab.Search.Indexer.Application.Data;

namespace MyLab.Search.Indexer.Application.Abstraction
{
    public interface ISeedStorage
    {
        Task<long> GetAsync(string idxId, CancellationToken ct);
        Task SetAsync(string idxId, long seed, CancellationToken ct);
    }
}
