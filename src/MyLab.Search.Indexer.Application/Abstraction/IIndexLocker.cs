namespace MyLab.Search.Indexer.Application.Abstraction;

public interface IIndexLocker
{
    Task<bool> TryLockAsync(string indexId, CancellationToken ct, out IIndexLock? unlocker);
}

public interface IIndexLock : IAsyncDisposable
{
    Task<bool> HeartbeatAsync(CancellationToken ct);
}