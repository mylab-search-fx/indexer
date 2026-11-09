using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Infrastructure.FileSystem;
using YamlDotNet.Core;

namespace MyLab.Search.Indexer.Infrastructure.UnitTests.FileSystem;

public partial class LocalFileIndexConfigProviderBehavior : IDisposable
{
    [Fact]
    public async Task ShouldThrowWhenWorkerOptionsAreMissing()
    {
        // Arrange
        var provider = new LocalFileIndexConfigProvider(new MissingWorkerOptions());

        // Act
        var error = await Record.ExceptionAsync(() => provider.ProvideAllAsync(CancellationToken.None));

        // Assert
        var invalid = Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Options not found", invalid.Message);
    }

    [Fact]
    public async Task ShouldThrowWhenConfigDirectoryDoesNotExist()
    {
        // Arrange
        var missingPath = Path.Combine(Path.GetTempPath(), "idx-cfg-missing-" + Guid.NewGuid().ToString("N"));
        var provider = CreateProvider(missingPath);

        // Act
        var error = await Record.ExceptionAsync(() => provider.ProvideAllAsync(CancellationToken.None));

        // Assert
        var missing = Assert.IsType<DirectoryNotFoundException>(error);
        Assert.Contains(missingPath, missing.Message);
    }

    [Fact]
    public async Task ShouldReturnEmptyListWhenConfigDirectoryHasNoIndexDirectories()
    {
        // Arrange
        var root = CreateConfigRoot();
        File.WriteAllText(Path.Combine(root, "query.sql"), "select 1");
        var provider = CreateProvider(root);

        // Act
        var result = await provider.ProvideAllAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ShouldLoadIndexConfigFromDirectoryFiles()
    {
        // Arrange
        var root = CreateConfigRoot();
        WriteIndex(
            root,
            "orders",
            "select id from orders where seed > @seed",
            """
            PageSize: 25
            SeepField: UpdatedAt
            """);
        var provider = CreateProvider(root);

        // Act
        var result = await provider.ProvideAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(
            [new IndexConfig("orders", "select id from orders where seed > @seed", new IndexParams(25, "UpdatedAt"))],
            result);
    }

    [Fact]
    public async Task ShouldKeepDefaultParamWhenYamlOmitsIt()
    {
        // Arrange
        var root = CreateConfigRoot();
        WriteIndex(root, "orders", "select id from orders", "PageSize: 25\n");
        var provider = CreateProvider(root);

        // Act
        var result = await provider.ProvideAllAsync(CancellationToken.None);

        // Assert
        var config = Assert.Single(result);
        Assert.Equal(new IndexParams(25, "Seed"), config.Params);
    }

    [Fact]
    public async Task ShouldThrowWhenParamsYamlIsInvalid()
    {
        // Arrange
        var root = CreateConfigRoot();
        WriteIndex(root, "orders", "select id from orders", "PageSize: [\n");
        var provider = CreateProvider(root);

        // Act
        var error = await Record.ExceptionAsync(() => provider.ProvideAllAsync(CancellationToken.None));

        // Assert
        Assert.IsType<YamlException>(error);
    }

    [Fact]
    public async Task ShouldUseDefaultParamsWhenParamsFileIsMissing()
    {
        // Arrange
        var root = CreateConfigRoot();
        WriteIndex(root, "orders", "select id from orders");
        var provider = CreateProvider(root);

        // Act
        var result = await provider.ProvideAllAsync(CancellationToken.None);

        // Assert
        var config = Assert.Single(result);
        Assert.Equal("orders", config.Name);
        Assert.Equal("select id from orders", config.SqlQuery);
        Assert.Equal(new IndexParams(), config.Params);
    }

    [Fact]
    public async Task ShouldThrowWhenQueryFileIsMissing()
    {
        // Arrange
        var root = CreateConfigRoot();
        var indexDir = Directory.CreateDirectory(Path.Combine(root, "orders")).FullName;
        var queryPath = Path.Combine(indexDir, "query.sql");
        File.WriteAllText(Path.Combine(indexDir, "params.yml"), "PageSize: 10\n");
        var provider = CreateProvider(root);

        // Act
        var error = await Record.ExceptionAsync(() => provider.ProvideAllAsync(CancellationToken.None));

        // Assert
        var missing = Assert.IsType<FileNotFoundException>(error);
        Assert.Equal(queryPath, missing.FileName);
        Assert.Contains("Query file not found", missing.Message);
    }

    [Fact]
    public async Task ShouldLoadConfigForEachIndexDirectory()
    {
        // Arrange
        var root = CreateConfigRoot();
        WriteIndex(root, "beta", "select b", "PageSize: 10\nSeepField: B\n");
        WriteIndex(root, "alpha", "select a");
        var provider = CreateProvider(root);

        // Act
        var result = await provider.ProvideAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(
            [
                new IndexConfig("alpha", "select a", new IndexParams()),
                new IndexConfig("beta", "select b", new IndexParams(10, "B"))
            ],
            result.OrderBy(config => config.Name, StringComparer.Ordinal).ToArray());
    }
}
