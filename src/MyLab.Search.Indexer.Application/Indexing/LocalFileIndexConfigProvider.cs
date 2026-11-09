using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Application.Options;
using YamlDotNet.Serialization;

namespace MyLab.Search.Indexer.Application.Indexing
{
    public class LocalFileIndexConfigProvider(IOptions<WorkerOptions> opts) : IIndexConfigProvider
    {
        readonly IDeserializer _yamlDeserializer = new DeserializerBuilder().Build();

        public async Task<IReadOnlyList<IndexConfig>> ProvideAllAsync(CancellationToken cancellationToken)
        {
            if (opts.Value == null)
                throw new InvalidOperationException("Options not found");

            var configDir = new DirectoryInfo(opts.Value.ConfigPath);
            var idxDirs = configDir.EnumerateDirectories();

            
            var idxConfigs = await Task.WhenAll(idxDirs.Select(GetIndexConfigAsync));

            return idxConfigs
                .ToList()
                .AsReadOnly();
        }

        async Task<IndexConfig> GetIndexConfigAsync(DirectoryInfo idxDir)
        {
            var name = idxDir.Name;
            var querySql = await LoadSqlQueryAsync(idxDir);
            var prms = await LoadParamsAsync(idxDir);

            return new IndexConfig(name, querySql, prms);
        }

        async Task<IndexParams> LoadParamsAsync(DirectoryInfo idxDir)
        {
            var paramsFilePath = Path.Combine(idxDir.FullName, "params.yml");
            var paramsFile = new FileInfo(paramsFilePath);

            if (!paramsFile.Exists)
                return new IndexParams();

            var paramsStr = await File.ReadAllTextAsync(paramsFilePath);

            return _yamlDeserializer.Deserialize<IndexParams>(paramsStr);
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
}
