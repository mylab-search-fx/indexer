namespace MyLab.Search.Indexer.Application.Data;

public static class DateTimeToIntConverter
{
    public static long DateTimeToLong(DateTime dateTime) => dateTime.Ticks;

    public static DateTime LongToDateTime(long value) => new(value);
}