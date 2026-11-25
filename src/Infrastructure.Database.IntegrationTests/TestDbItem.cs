using LinqToDB.Mapping;

namespace Infrastructure.Database.IntegrationTests;

[Table("item")]
public class TestDbItem
{
    [Column("id", IsPrimaryKey = true)]
    public int Id { get; set; }
    [Column("val")]
    public string? Value { get; set; }
    [Column("last_modified")]
    public DateTime LastModified { get; set; }
}