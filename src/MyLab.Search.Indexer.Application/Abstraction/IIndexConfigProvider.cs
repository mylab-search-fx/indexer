using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Application.Abstraction
{
    public interface IIndexConfigProvider
    {
        Task<IReadOnlyList<IndexConfig>> ProvideAllAsync(CancellationToken cancellationToken);
    }
}
