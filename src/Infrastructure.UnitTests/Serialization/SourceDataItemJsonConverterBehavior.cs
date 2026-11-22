using System.Text;
using System.Text.Json;
using MyLab.Search.Indexer.Application.Data;
using MyLab.Search.Indexer.Infrastructure.OpenSearch.Serialization;

namespace Infrastructure.UnitTests.Serialization;

public class SourceDataItemJsonConverterBehavior
{
    [Fact]
    public void ShouldWriteBooleanValue()
    {
        // Arrange
        var item = Item("active", new SourceDataValue(SourceDataValueKind.Bool, true));

        // Act
        var json = WriteJson(item);

        using var document = JsonDocument.Parse(json);
        var field = document.RootElement.GetProperty("active");
        var kind = field.ValueKind;

        // Assert
        Assert.Equal(JsonValueKind.True, kind);
    }

    [Fact]
    public void ShouldWriteNullValue()
    {
        // Arrange
        var item = Item("note", new SourceDataValue(SourceDataValueKind.Null));

        // Act
        var json = WriteJson(item);

        using var document = JsonDocument.Parse(json);
        var field = document.RootElement.GetProperty("note");
        var kind = field.ValueKind;

        // Assert
        Assert.Equal(JsonValueKind.Null, kind);
    }

    [Fact]
    public void ShouldWriteNumberValue()
    {
        // Arrange
        var item = Item("amount", new SourceDataValue(SourceDataValueKind.Number, 42));

        // Act
        var json = WriteJson(item);

        using var document = JsonDocument.Parse(json);
        var field = document.RootElement.GetProperty("amount");
        var value = field.GetInt32();

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void ShouldWriteFloatValue()
    {
        // Arrange
        var item = Item("ratio", new SourceDataValue(SourceDataValueKind.Float, 2.5d));

        // Act
        var json = WriteJson(item);

        using var document = JsonDocument.Parse(json);
        var field = document.RootElement.GetProperty("ratio");
        var value = field.GetDouble();

        // Assert
        Assert.Equal(2.5d, value);
    }

    [Fact]
    public void ShouldWriteStringValue()
    {
        // Arrange
        var item = Item("title", new SourceDataValue(SourceDataValueKind.String, "alpha"));

        // Act
        var json = WriteJson(item);

        using var document = JsonDocument.Parse(json);
        var field = document.RootElement.GetProperty("title");
        var value = field.GetString();

        // Assert
        Assert.Equal("alpha", value);
    }

    [Fact]
    public void ShouldWriteDateTimeValue()
    {
        // Arrange
        var moment = new DateTime(2026, 11, 23, 4, 5, 6);
        var item = Item("updated_at", new SourceDataValue(SourceDataValueKind.DateTime, moment));

        // Act
        var json = WriteJson(item);

        using var document = JsonDocument.Parse(json);
        var field = document.RootElement.GetProperty("updated_at");
        var value = field.GetString();

        // Assert
        Assert.Equal(moment.ToString("s"), value);
    }

    static SourceDataItem Item(string name, SourceDataValue value) => new()
    {
        Id = new SourceDataValue(SourceDataValueKind.String, "id-1"),
        Seed = 1,
        Fields = new Dictionary<string, SourceDataValue> { [name] = value }
    };

    static string WriteJson(SourceDataItem item)
    {
        var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        var converter = new SourceDataItemJsonConverter();
        converter.Write(writer, item, new JsonSerializerOptions());
        writer.WriteEndObject();
        writer.Flush();
        var json = Encoding.UTF8.GetString(stream.ToArray());
        return json;
    }
}
