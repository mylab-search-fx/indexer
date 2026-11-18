using System.Data;
using System.Data.Common;
using MyLab.Search.Indexer.Application.Data;
using MyLab.Search.Indexer.Infrastructure.Database.SourceRepository;

namespace Infrastructure.UnitTests.SourceRepository;

public class SourceDataItemReaderBehavior
{
    [Fact]
    public void ShouldReadSupportedPrimitiveFields()
    {
        // Arrange
        var moment = new DateTime(2026, 11, 18, 8, 3, 4);
        using var reader = OpenRow(
            ("id", typeof(int), 7),
            ("title", typeof(string), "alpha"),
            ("amount", typeof(decimal), 12.5m),
            ("ratio", typeof(double), 1.25d),
            ("active", typeof(bool), true),
            ("updated_at", typeof(DateTime), moment),
            ("note", typeof(string), null),
            ("new_seed", typeof(long), 20L));

        // Act
        var item = SourceDataItemReader.ReadItem(reader);

        // Assert
        Assert.Equal(SourceDataValue.ForValue(7), item.Id);
        Assert.Equal(20L, item.Seed);
        Assert.Equal(6, item.Fields.Count);
        Assert.Equal(SourceDataValue.ForValue("alpha"), item.Fields["title"]);
        Assert.Equal(SourceDataValue.ForValue(12.5m), item.Fields["amount"]);
        Assert.Equal(SourceDataValue.ForValue(1.25d), item.Fields["ratio"]);
        Assert.Equal(SourceDataValue.ForValue(true), item.Fields["active"]);
        Assert.Equal(SourceDataValue.ForValue(moment), item.Fields["updated_at"]);
        Assert.Equal(SourceDataValue.Null, item.Fields["note"]);
    }

    [Fact]
    public void ShouldReadStringId()
    {
        // Arrange
        using var reader = OpenRow(
            ("id", typeof(string), "order-1"),
            ("new_seed", typeof(long), 5L));

        // Act
        var item = SourceDataItemReader.ReadItem(reader);

        // Assert
        Assert.Equal(SourceDataValue.ForValue("order-1"), item.Id);
        Assert.Equal(5L, item.Seed);
        Assert.Empty(item.Fields);
    }

    [Fact]
    public void ShouldReadIntSeed()
    {
        // Arrange
        using var reader = OpenRow(
            ("id", typeof(int), 1),
            ("new_seed", typeof(int), 15));

        // Act
        var item = SourceDataItemReader.ReadItem(reader);

        // Assert
        Assert.Equal(15L, item.Seed);
    }

    [Fact]
    public void ShouldConvertDateTimeSeedToTicks()
    {
        // Arrange
        var moment = new DateTime(2026, 11, 18, 8, 3, 4);
        using var reader = OpenRow(
            ("id", typeof(int), 1),
            ("new_seed", typeof(DateTime), moment));

        // Act
        var item = SourceDataItemReader.ReadItem(reader);

        // Assert
        Assert.Equal(moment.Ticks, item.Seed);
    }

    [Fact]
    public void ShouldThrowWhenIdIsMissing()
    {
        // Arrange
        using var reader = OpenRow(("new_seed", typeof(long), 1L));

        // Act
        var error = Record.Exception(() => SourceDataItemReader.ReadItem(reader));

        // Assert
        var invalid = Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Records without the 'id' field have been received", invalid.Message);
    }

    [Fact]
    public void ShouldThrowWhenIdIsNull()
    {
        // Arrange
        using var reader = OpenRow(
            ("id", typeof(int), null),
            ("new_seed", typeof(long), 1L));

        // Act
        var error = Record.Exception(() => SourceDataItemReader.ReadItem(reader));

        // Assert
        var invalid = Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Records with NULL 'id' field have been received", invalid.Message);
    }

    [Fact]
    public void ShouldThrowWhenIdTypeIsWrong()
    {
        // Arrange
        using var reader = OpenRow(
            ("id", typeof(bool), true),
            ("new_seed", typeof(long), 1L));

        // Act
        var error = Record.Exception(() => SourceDataItemReader.ReadItem(reader));

        // Assert
        var invalid = Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Records with the 'id' field with wrong type have been received", invalid.Message);
    }

    [Fact]
    public void ShouldThrowWhenSeedIsMissing()
    {
        // Arrange
        using var reader = OpenRow(("id", typeof(int), 1));

        // Act
        var error = Record.Exception(() => SourceDataItemReader.ReadItem(reader));

        // Assert
        var invalid = Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Records without the 'new_seed' field have been received", invalid.Message);
    }

    [Fact]
    public void ShouldThrowWhenSeedIsNull()
    {
        // Arrange
        using var reader = OpenRow(
            ("id", typeof(int), 1),
            ("new_seed", typeof(long), null));

        // Act
        var error = Record.Exception(() => SourceDataItemReader.ReadItem(reader));

        // Assert
        var invalid = Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Records with NULL 'new_seed' field have been received", invalid.Message);
    }

    [Fact]
    public void ShouldThrowWhenSeedNumberTypeIsUnsupported()
    {
        // Arrange
        using var reader = OpenRow(
            ("id", typeof(int), 1),
            ("new_seed", typeof(double), 1.5d));

        // Act
        var error = Record.Exception(() => SourceDataItemReader.ReadItem(reader));

        // Assert
        var invalid = Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Unsupported field type: System.Double", invalid.Message);
    }

    [Fact]
    public void ShouldThrowWhenSeedKindIsUnsupported()
    {
        // Arrange
        using var reader = OpenRow(
            ("id", typeof(int), 1),
            ("new_seed", typeof(string), "later"));

        // Act
        var error = Record.Exception(() => SourceDataItemReader.ReadItem(reader));

        // Assert
        var invalid = Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Unsupported field kind: String", invalid.Message);
    }

    [Fact]
    public void ShouldThrowWhenFieldTypeIsUnsupported()
    {
        // Arrange
        using var reader = OpenRow(
            ("id", typeof(int), 1),
            ("flag", typeof(byte), (byte)1),
            ("new_seed", typeof(long), 1L));

        // Act
        var error = Record.Exception(() => SourceDataItemReader.ReadItem(reader));

        // Assert
        var invalid = Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Unsupported field type: System.Byte", invalid.Message);
    }

    static DbDataReader OpenRow(params (string Name, Type Type, object? Value)[] columns)
    {
        var table = new DataTable();
        foreach (var column in columns)
        {
            var dataColumn = table.Columns.Add(column.Name, column.Type);
            dataColumn.AllowDBNull = true;
        }

        var values = new object?[columns.Length];
        for (var i = 0; i < columns.Length; i++)
            values[i] = columns[i].Value ?? DBNull.Value;

        table.Rows.Add(values);
        var reader = table.CreateDataReader();
        var hasRow = reader.Read();
        if (!hasRow)
            throw new InvalidOperationException("Row was not read.");

        return reader;
    }
}
