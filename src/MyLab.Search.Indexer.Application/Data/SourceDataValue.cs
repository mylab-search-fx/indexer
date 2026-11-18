namespace MyLab.Search.Indexer.Application.Data;

public record SourceDataValue(SourceDataValueKind Kind, object? Value = null)
{
    public static readonly SourceDataValue Null = new (SourceDataValueKind.Null);

    public static SourceDataValue ForValue(object? value)
    {
        if (value == null)
            return Null;

        switch (Type.GetTypeCode(value.GetType()))
        {
            case TypeCode.Int32:
                return new SourceDataValue(SourceDataValueKind.Number, (int)value);
            case TypeCode.Int64:
                return new SourceDataValue(SourceDataValueKind.Number, (long)value);
            case TypeCode.Double:
                return new SourceDataValue(SourceDataValueKind.Number, (double)value);
            case TypeCode.Decimal:
                return new SourceDataValue(SourceDataValueKind.Number, (decimal)value);
            case TypeCode.Boolean:
                return new SourceDataValue(SourceDataValueKind.Bool, (bool)value);
            case TypeCode.String:
                return new SourceDataValue(SourceDataValueKind.String, (string)value);
            case TypeCode.DateTime:
                return new SourceDataValue(SourceDataValueKind.DateTime, (DateTime)value);
            default:
            {
                throw new InvalidOperationException("Unsupported field type: " + value.GetType().FullName);
            }
        }
    }
}