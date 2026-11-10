using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Application.Options;
using Vaxquis.NanoYaml;
using YamlDotNet.Serialization;

namespace MyLab.Search.Indexer.Infrastructure.FileSystem;

public class LocalFileIndexDescProvider(IOptions<WorkerOptions> opts) : IIndexDescProvider
{
    //readonly IDeserializer _yamlDeserializer = new DeserializerBuilder().Build();

    public async Task<IReadOnlyList<IndexDescription>> ProvideAllAsync(CancellationToken cancellationToken)
    {
        if (opts.Value == null)
            throw new InvalidOperationException("Options not found");

        var configDir = new DirectoryInfo(opts.Value.ConfigPath);
        var idxDirs = configDir.EnumerateDirectories();

            
        var idxDescs = await Task.WhenAll(idxDirs.Select(GetIndexDescAsync));

        return idxDescs
            .ToList()
            .AsReadOnly();
    }

    async Task<IndexDescription> GetIndexDescAsync(DirectoryInfo idxDir)
    {
        var name = idxDir.Name;
        var querySql = await LoadSqlQueryAsync(idxDir);
        var prms = await LoadParamsAsync(idxDir);

        return new IndexDescription(name, querySql, prms);
    }

    async Task<IndexConfig> LoadParamsAsync(DirectoryInfo idxDir)
    {
        var paramsFilePath = Path.Combine(idxDir.FullName, "params.yml");
        var paramsFile = new FileInfo(paramsFilePath);

        if (!paramsFile.Exists)
            return new IndexConfig();

        var paramsStr = await File.ReadAllTextAsync(paramsFilePath);

        return NanoYaml.Deserialize<IndexConfig>(paramsStr);
    }

    Task<string> LoadSqlQueryAsync(DirectoryInfo idxDir)
    {
        var queryFilePath = Path.Combine(idxDir.FullName, "query.sql");
        var queryFile = new FileInfo(queryFilePath);

        if (!queryFile.Exists)
            throw new FileNotFoundException("Query file not found", queryFilePath);

        return File.ReadAllTextAsync(queryFilePath);
    }
}