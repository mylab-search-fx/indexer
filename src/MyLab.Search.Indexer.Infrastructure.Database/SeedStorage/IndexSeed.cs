using LinqToDB.Mapping;

namespace MyLab.Search.Indexer.Infrastructure.Database.SeedStorage;

[Table("seed")]
public class IndexSeed
{
    [Column("idx", IsPrimaryKey = true)]
    public required string IndexId { get; set; }
    [Column("value")]
    public long Value { get; set; }
}