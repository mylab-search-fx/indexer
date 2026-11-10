using MyLab.Search.Indexer.Application.Abstraction;

namespace MyLab.Search.Indexer.Infrastructure.Redis;

class RedlockIndexLocker : IIndexLocker
{
    public Task<bool> TryLockAsync(string indexId, CancellationToken ct, out IIndexLock? unlocker)
    {
        throw new NotImplementedException();
    }
}