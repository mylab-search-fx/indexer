namespace MyLab.Search.Indexer.Application.Abstraction;

public interface IIndexLocker
{
    Task<IIndexLock> TryLockAsync(string indexId, CancellationToken ct);
}

public interface IIndexLock : IAsyncDisposable
{
    bool Locked { get; }
    Task<bool> HeartbeatAsync(CancellationToken ct);
}