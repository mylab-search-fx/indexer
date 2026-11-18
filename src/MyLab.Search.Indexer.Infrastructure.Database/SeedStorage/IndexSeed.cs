using LinqToDB.Mapping;

namespace MyLab.Search.Indexer.Infrastructure.Database.SeedStorage;

[Table("seed")]
public class IndexSeed
{
    [Column("idx", IsPrimaryKey = true, Length = 255)]
    public required string IndexId { get; set; }
    [Column("value")]
    public long Value { get; set; }
}