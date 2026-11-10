using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Application.Abstraction;

public interface IIndexDescProvider
{
    Task<IReadOnlyList<IndexDescription>> ProvideAllAsync(CancellationToken cancellationToken);
}