using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Infrastructure.FileSystem;

class LocalFileIndexDescProvider(IOptions<WorkerFileSystemOptions> opts) : IIndexDescProvider
{
    public Task<IReadOnlyList<IndexDescription>> ProvideAllAsync(CancellationToken cancellationToken)
    {
        if (opts.Value == null)
            throw new InvalidOperationException("Options not found");

        return IndexDescriptionFileTool.ReadAllIndexDescAsync(opts.Value.ConfigPath);
    }
}