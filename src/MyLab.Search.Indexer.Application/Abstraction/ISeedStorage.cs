namespace MyLab.Search.Indexer.Application.Abstraction
{
    public interface ISeedStorage
    {
        Task<long> GetAsync(string idxId);
        Task SetAsync(string idxId, long seed);
    }
}
