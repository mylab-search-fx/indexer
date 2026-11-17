using LinqToDB.Mapping;

namespace MyLab.Search.Indexer.Infrastructure.DB.IndexVersionControl;

[Table("versions")]
public class IndexVersion
{
    [Column("val", IsPrimaryKey = true)]
    public string? Value { get; set; }
    [Column("reg_dt")]
    public DateTime Registered { get; set; }
}