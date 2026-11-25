using LinqToDB.Data;
using MyLab.Db;
using MyLab.Search.Indexer.Application.Data;
using MyLab.Search.Indexer.Infrastructure.Database.SourceRepository;
using Xunit.Abstractions;

namespace Infrastructure.Database.IntegrationTests.SourceRepository;

public class SourceDataItemReaderBehavior : IClassFixture<LocalServerDbFixture>
{
    readonly IDbManager _db;

    public SourceDataItemReaderBehavior(LocalServerDbFixture fxt, ITestOutputHelper output)
    {
        fxt.Output = output;
        _db = fxt.Manager;
    }

    [Fact(Timeout = 10000)]
    public async Task ShouldReadPrimitiveFieldTypesFromDatabase()
    {
        // Arrange
        await using var dataRdr = await _db.DoOnce().ExecuteReaderAsync(
            """
                 SELECT
                 1 as id,
                 7 as int_value,
                 CAST(3000000000 AS SIGNED) AS long_value,        
                 CAST(1.25 AS DOUBLE) AS double_value,     
                 CAST(12.5 AS DECIMAL(10, 2)) AS decimal_value,
                 CAST('alpha' AS CHAR(100)) AS string_value,
                 CAST('2026-11-18 08:03:04' AS DATETIME) AS datetime_value,
                 NULL as null_value,
                 CAST(20 AS SIGNED) AS new_seed;
                 """);

        if (dataRdr.Reader == null)
            throw new InvalidOperationException("dataRdr.Reader == null");

        await dataRdr.Reader.ReadAsync();

        // Act
        var item = SourceDataItemReader.ReadItem(dataRdr.Reader);

        // Assert
        Assert.Equal(SourceDataValue.ForValue(1), item.Id);
        Assert.Equal(20L, item.Seed);
        Assert.Equal(7, item.Fields.Count);
        Assert.Equal(SourceDataValue.ForValue(7), item.Fields["int_value"]);
        Assert.Equal(SourceDataValue.ForValue(3000000000L), item.Fields["long_value"]);
        Assert.Equal(SourceDataValue.ForValue(1.25d), item.Fields["double_value"]);
        Assert.Equal(SourceDataValue.ForValue(12.5m), item.Fields["decimal_value"]);
        Assert.Equal(SourceDataValue.ForValue("alpha"), item.Fields["string_value"]);
        Assert.Equal(SourceDataValue.ForValue(new DateTime(2026, 11, 18, 8, 3, 4)), item.Fields["datetime_value"]);
        Assert.Equal(SourceDataValue.Null, item.Fields["null_value"]);
    }
}
