using MyLab.Search.Indexer.Application.Data;
using System.Data.Common;

namespace MyLab.Search.Indexer.Infrastructure.Database.SourceRepository;

static class SourceDataItemReader
{
    public static SourceDataItem ReadItem(DbDataReader rdr)
    {
        var fields = ReadFields(rdr);
        var id = ExtractId(fields);
        var seed = ExtractSeed(fields);

        return new SourceDataItem
        {
            Id = id,
            Seed = seed,
            Fields = fields.ToDictionary(f => f.Name, f => f.Value)
        };
    }

    static SourceDataValue ExtractId(List<DbField> fields)
    {
        var idField = fields.FirstOrDefault(f => f.Name == "id");

        if (idField == null)
            throw new InvalidOperationException("Records without the 'id' field have been received");

        if (idField.Value.Kind == SourceDataValueKind.Null)
            throw new InvalidOperationException("Records with NULL 'id' field have been received");

        if (idField.Value.Kind != SourceDataValueKind.Number && idField.Value.Kind != SourceDataValueKind.String)
            throw new InvalidOperationException("Records with the 'id' field with wrong type have been received");

        fields.Remove(idField);
        return idField.Value;
    }

    static long ExtractSeed(List<DbField> fields)
    {
        var seedField = fields.FirstOrDefault(f => f.Name == "new_seed");

        if (seedField == null)
            throw new InvalidOperationException("Records without the 'new_seed' field have been received");

        long seedValue;

        switch (seedField.Value.Kind)
        {
            case SourceDataValueKind.Null:
                throw new InvalidOperationException("Records with NULL 'new_seed' field have been received");
            case SourceDataValueKind.Number:
            {
                switch (Type.GetTypeCode(seedField.Value.Value!.GetType()))
                {
                        case TypeCode.Int32:
                            seedValue = (int)seedField.Value.Value;
                            break;
                        case TypeCode.Int64:
                            seedValue = (long)seedField.Value.Value;
                            break;
                        default:
                            throw new InvalidOperationException("Unsupported field type: " + seedField.Value.Value!.GetType().FullName);
                    }
            }
                break;
            case SourceDataValueKind.DateTime:
            {
                var seedValueLong = DateTimeToIntConverter.DateTimeToLong((DateTime)seedField.Value.Value!);
                seedValue = seedValueLong;
            }
                break;
            default:
                throw new InvalidOperationException("Unsupported field kind: " + Enum.GetName(seedField.Value.Kind));
        }

        fields.Remove(seedField);
        return seedValue;
    }

    static List<DbField> ReadFields(DbDataReader rdr)
    {
        var fields = new List<DbField>();

        for (int ordinal = 0; ordinal < rdr.FieldCount; ordinal++)
        {
            var fName = rdr.GetName(ordinal);

            if (rdr.IsDBNull(ordinal))
            {
                fields.Add(new DbField(fName, SourceDataValue.Null));
                continue;
            }

            var fieldType = rdr.GetFieldType(ordinal);

            object fieldValue;

            switch (Type.GetTypeCode(fieldType))
            {
                case TypeCode.Int32:
                {
                    fieldValue = rdr.GetInt32(ordinal);
                }
                break;
                case TypeCode.Int64:
                {
                    fieldValue = rdr.GetInt64(ordinal);
                }
                break;
                case TypeCode.Double:
                {
                    fieldValue = rdr.GetDouble(ordinal);
                }
                break;
                case TypeCode.Decimal:
                {
                    fieldValue = rdr.GetDecimal(ordinal);
                }
                break;
                case TypeCode.String:
                {
                    fieldValue = rdr.GetString(ordinal);
                } 
                break;
                case TypeCode.Boolean:
                {
                    fieldValue = rdr.GetBoolean(ordinal);
                }
                break;
                case TypeCode.DateTime:
                {
                    fieldValue = rdr.GetDateTime(ordinal);
                }
                break;
                default:
                {
                    throw new InvalidOperationException("Unsupported field type: " + fieldType.FullName);
                }
            }

            var val = SourceDataValue.ForValue(fieldValue);
            fields.Add(new DbField(fName, val));
        }

        return fields;
    }

    record DbField(string Name, SourceDataValue Value);
}