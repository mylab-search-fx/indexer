using LinqToDB.Data;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Infrastructure.Database.SourceRepository;

namespace Infrastructure.Database.IntegrationTests.SourceRepository;

[Collection(DatabaseCollection.Name)]
public partial class DbSourceRepositoryBehavior : IAsyncLifetime
{
    [Fact(Timeout = 10000)]
    public async Task ShouldThrowWhenReadingRowAsJsonDocument()
    {
        // Arrange
        await using (var source = _db.Use(TestTools.SourceConnection))
        {
            await source.ExecuteAsync(
                """
                CREATE TABLE orders
                (
                    id    INT          NOT NULL,
                    title VARCHAR(100) NOT NULL,
                    seed  BIGINT       NOT NULL,
                    PRIMARY KEY (id)
                )
                """);
            await source.ExecuteAsync(
                """
                INSERT INTO orders (id, title, seed)
                VALUES (1, 'alpha', 10)
                """);
        }

        var index = new IndexDescription(
            "orders",
            $"""
             SELECT id, title, seed AS new_seed
             FROM {TestTools.SourceDatabase}.orders
             WHERE seed > @seed
             ORDER BY seed
             LIMIT @limit
             """,
            new IndexConfig(PageSize: 10));
        var repository = new DbSourceRepository(index, _db);

        // Act
        var data = await repository.ReadAsync(0, CancellationToken.None);

        // Assert
    }
}
