using LinqToDB.Mapping;

namespace MyLab.Search.Indexer.Infrastructure.Database.IndexVersionControl;

[Table("version")]
public class IndexVersion
{
    [Column("val", IsPrimaryKey = true, Length = 255)]
    public required string Value { get; set; }
    [Column("reg_dt")]
    public DateTime Registered { get; set; }
}