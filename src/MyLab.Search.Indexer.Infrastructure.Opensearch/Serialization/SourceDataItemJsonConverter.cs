using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using MyLab.Search.Indexer.Application.Data;

namespace MyLab.Search.Indexer.Infrastructure.OpenSearch.Serialization;

class SourceDataItemJsonConverter : JsonConverter<SourceDataItem>
{
    public override SourceDataItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, SourceDataItem value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        WriteField(writer, "id", value.Id);
        foreach (var field in value.Fields)
        {
            WriteField(writer, field.Key, field.Value);
        }
        writer.WriteEndObject();
    }

    void WriteField(Utf8JsonWriter writer, string name, SourceDataValue value)
    {
        if (value.Value == null)
        {
            writer.WriteNull(name);
            return;
        }

        switch (value.Kind)
        {
            case SourceDataValueKind.Bool:
                writer.WriteBoolean(name, (bool)value.Value);
                break;
            case SourceDataValueKind.Null:
                writer.WriteNull(name);
                break;
            case SourceDataValueKind.Float:
            case SourceDataValueKind.Number:
                writer.WritePropertyName(name);
                writer.WriteRawValue(Convert.ToString(value.Value, CultureInfo.InvariantCulture)!);
                break;
            case SourceDataValueKind.String:
                writer.WriteString(name, (string)value.Value);
                break;
            case SourceDataValueKind.DateTime:
                writer.WriteString(name, ((DateTime)value.Value).ToString("s"));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}