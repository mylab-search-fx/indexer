using LinqToDB.Data;
using MyLab.Search.Indexer.Application.Data;
using MyLab.Search.Indexer.Infrastructure.Database.SourceRepository;

namespace Infrastructure.Database.IntegrationTests.SourceRepository;

[Collection(DatabaseCollection.Name)]
public partial class SourceDataItemReaderBehavior : IAsyncLifetime
{
    [Fact(Timeout = 10000)]
    public async Task ShouldReadPrimitiveFieldTypesFromDatabase()
    {
        // Arrange
        var moment = new DateTime(2026, 11, 18, 8, 3, 4);
        await using (var source = _db.Use(TestTools.SourceConnection))
        {
            await source.ExecuteAsync(
                """
                CREATE TABLE primitive_values
                (
                    id             INT           NOT NULL,
                    int_value      INT           NOT NULL,
                    long_value     BIGINT        NOT NULL,
                    double_value   DOUBLE        NOT NULL,
                    decimal_value  DECIMAL(18,4) NOT NULL,
                    string_value   VARCHAR(100)  NOT NULL,
                    bool_value     BOOLEAN       NOT NULL,
                    datetime_value DATETIME      NOT NULL,
                    null_value     VARCHAR(100)  NULL,
                    new_seed       BIGINT        NOT NULL,
                    PRIMARY KEY (id)
                )
                """);
            await source.ExecuteAsync(
                """
                INSERT INTO primitive_values
                (
                    id, int_value, long_value, double_value, decimal_value,
                    string_value, bool_value, datetime_value, null_value, new_seed
                )
                VALUES
                (
                    1, 7, 3000000000, 1.25, 12.5,
                    'alpha', TRUE, '2026-11-18 08:03:04', NULL, 20
                )
                """);
        }

        var opened = await _db.DoOnce().ExecuteReaderAsync(
            $"""
             SELECT
                 id,
                 int_value,
                 long_value,
                 double_value,
                 decimal_value,
                 string_value,
                 bool_value,
                 datetime_value,
                 null_value,
                 new_seed
             FROM {TestTools.SourceDatabase}.primitive_values
             """);
        await using var query = opened;
        var dbReader = query.Reader;
        if (dbReader == null)
            throw new InvalidOperationException("Reader was not opened.");
        var hasRow = await dbReader.ReadAsync();
        if (!hasRow)
            throw new InvalidOperationException("Row was not read.");

        // Act
        var item = SourceDataItemReader.ReadItem(dbReader);

        // Assert
        Assert.Equal(SourceDataValue.ForValue(1), item.Id);
        Assert.Equal(20L, item.Seed);
        Assert.Equal(8, item.Fields.Count);
        Assert.Equal(SourceDataValue.ForValue(7), item.Fields["int_value"]);
        Assert.Equal(SourceDataValue.ForValue(3000000000L), item.Fields["long_value"]);
        Assert.Equal(SourceDataValue.ForValue(1.25d), item.Fields["double_value"]);
        Assert.Equal(SourceDataValue.ForValue(12.5m), item.Fields["decimal_value"]);
        Assert.Equal(SourceDataValue.ForValue("alpha"), item.Fields["string_value"]);
        Assert.Equal(SourceDataValue.ForValue(true), item.Fields["bool_value"]);
        Assert.Equal(SourceDataValue.ForValue(moment), item.Fields["datetime_value"]);
        Assert.Equal(SourceDataValue.Null, item.Fields["null_value"]);
    }
}
