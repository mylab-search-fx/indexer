using MyLab.Search.Indexer.Application.Data;

namespace MyLab.Search.Indexer.Application.Abstraction
{
    public interface ISourceRepository
    {
        Task<SourceData> ReadAsync(long seed, CancellationToken cancellationToken);
    }
}
