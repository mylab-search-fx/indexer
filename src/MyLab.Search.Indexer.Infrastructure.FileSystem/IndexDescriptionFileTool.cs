using MyLab.Search.Indexer.Application.Config;
using Vaxquis.NanoYaml;

namespace MyLab.Search.Indexer.Infrastructure.FileSystem;

public class IndexDescriptionFileTool
{
    public static Task WriteAllIndexDescAsync(IEnumerable<IndexDescription> descs, string cfgDir)
    {
        var cfgDirInfo = new DirectoryInfo(cfgDir);
        if (!cfgDirInfo.Exists) throw new InvalidOperationException("Configuration directory not found");

        return Task.WhenAll(descs.Select(d => WriteIndexDescAsync(d, cfgDirInfo)));
    }

    public static async Task WriteIndexDescAsync(IndexDescription desc, DirectoryInfo cfgDir)
    {
        var descDir = cfgDir.CreateSubdirectory(desc.Name);
        var yaml = NanoYaml.Serialize(desc.Config);

        var cfgFilename = Path.Combine(descDir.FullName, Filenames.Config);
        await File.WriteAllTextAsync(cfgFilename, yaml);

        var queryFilename = Path.Combine(descDir.FullName, Filenames.Query);
        await File.WriteAllTextAsync(queryFilename, desc.SqlQuery);
    }

    public static async Task<IReadOnlyList<IndexDescription>> ReadAllIndexDescAsync(string cfgDir)
    {
        var cfgDirInfo = new DirectoryInfo(cfgDir);

        if (!cfgDirInfo.Exists) throw new InvalidOperationException("Configuration directory not found");

        var idxDirs = cfgDirInfo.EnumerateDirectories();
        var idxDescs = await Task.WhenAll(idxDirs.Select(ReadIndexDescAsync));

        return idxDescs
            .ToList()
            .AsReadOnly();
    }

    static async Task<IndexDescription> ReadIndexDescAsync(DirectoryInfo idxDir)
    {
        var name = idxDir.Name;
        var querySql = await LoadSqlQueryAsync(idxDir);
        var prms = await LoadConfigAsync(idxDir);

        return new IndexDescription(name, querySql, prms);
    }

    static async Task<IndexConfig> LoadConfigAsync(DirectoryInfo idxDir)
    {
        var paramsFilePath = Path.Combine(idxDir.FullName, Filenames.Config);
        var paramsFile = new FileInfo(paramsFilePath);

        if (!paramsFile.Exists)
            return new IndexConfig();

        var paramsStr = await File.ReadAllTextAsync(paramsFilePath);

        return NanoYaml.Deserialize<IndexConfig>(paramsStr);
    }

    static Task<string> LoadSqlQueryAsync(DirectoryInfo idxDir)
    {
        var queryFilePath = Path.Combine(idxDir.FullName, Filenames.Query);
        var queryFile = new FileInfo(queryFilePath);

        if (!queryFile.Exists)
            throw new FileNotFoundException("Query file not found", queryFilePath);

        return File.ReadAllTextAsync(queryFilePath);
    }
}