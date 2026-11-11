using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Application.Indexing;
using MyLab.Search.Indexer.Application.Options;
using MyLab.Search.Indexer.Infrastructure.FileSystem;

namespace MyLab.Search.Indexer.Infrastructure.UnitTests.FileSystem;

public partial class LocalFileIndexConfigProviderBehavior
{
    readonly List<string> _tempDirectories = [];

    public void Dispose()
    {
        foreach (var directory in _tempDirectories)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    string CreateConfigRoot()
    {
        var path = Directory.CreateTempSubdirectory("idx-cfg-").FullName;
        _tempDirectories.Add(path);
        return path;
    }

    static void WriteIndex(string root, string name, string sql, string? yaml = null)
    {
        var indexDir = Directory.CreateDirectory(Path.Combine(root, name)).FullName;
        File.WriteAllText(Path.Combine(indexDir, "query.sql"), sql);

        if (yaml != null)
            File.WriteAllText(Path.Combine(indexDir, "params.yml"), yaml);
    }

    static LocalFileIndexDescProvider CreateProvider(string configPath) =>
        new(new FixedWorkerOptions(new WorkerFileSystemOptions { ConfigPath = configPath }));

    sealed class FixedWorkerOptions(WorkerFileSystemOptions value) : IOptions<WorkerFileSystemOptions>
    {
        public WorkerFileSystemOptions Value => value;
    }

    sealed class MissingWorkerOptions : IOptions<WorkerFileSystemOptions>
    {
        public WorkerFileSystemOptions Value => null!;
    }
}