using System.Text.Json.Serialization;

namespace Worker.E2ETests;

public class TestOpenSearchItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("val")]
    public string? Value { get; set; }
    [JsonPropertyName("last_modified")]
    public DateTime LastModified { get; set; }
}