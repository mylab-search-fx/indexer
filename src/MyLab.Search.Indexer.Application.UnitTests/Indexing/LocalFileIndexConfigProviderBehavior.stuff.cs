using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Application.Indexing;
using MyLab.Search.Indexer.Application.Options;

namespace MyLab.Search.Indexer.Application.UnitTests.Indexing;

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

    static LocalFileIndexConfigProvider CreateProvider(string configPath) =>
        new(new FixedWorkerOptions(new WorkerOptions { ConfigPath = configPath }));

    sealed class FixedWorkerOptions(WorkerOptions value) : IOptions<WorkerOptions>
    {
        public WorkerOptions Value => value;
    }

    sealed class MissingWorkerOptions : IOptions<WorkerOptions>
    {
        public WorkerOptions Value => null!;
    }
}