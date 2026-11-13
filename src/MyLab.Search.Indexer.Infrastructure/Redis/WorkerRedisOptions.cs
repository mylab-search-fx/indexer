using MyLab.Search.Indexer.Application.Options;

namespace MyLab.Search.Indexer.Infrastructure.Redis;

public class WorkerRedisOptions
{
    public static readonly string SectionName = $"{WorkerOptions.SectionName}:Redis";

    public string? Host { get; set; }
    public int Port { get; set; } = 6379;
    public string? User { get; set; }
    public string? Password { get; set; }
    public int RedlockExpirySec { get; set; } = 60;
}