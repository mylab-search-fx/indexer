using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Application.Data;
using MyLab.Search.Indexer.Application.Indexing;

namespace MyLab.Search.Indexer.Application.UnitTests.Indexing;

public partial class IndexWorkerBehavior
{
    sealed class Fixture
    {
        readonly Queue<bool> _versionOk = new();
        readonly Queue<bool> _lockGrants = new();
        readonly Queue<bool> _heartbeats = new();
        readonly Queue<SourceData> _pages = new();
        Exception? _writeError;

        public IndexDescription Index { get; } = new("orders", "select id", new IndexConfig(Idle: 0));

        public string Version { get; set; } = "v1";

        public long Seed { get; set; }

        public List<IndexDescription> VersionRequests { get; } = [];

        public List<string> CheckedOutVersions { get; } = [];

        public List<string> LockedIndexes { get; } = [];

        public List<long> ReadSeeds { get; } = [];

        public List<SourceData> WrittenPages { get; } = [];

        public List<string> LoadedSeedIndexes { get; } = [];

        public List<long> SavedSeeds { get; } = [];

        public List<string> SavedSeedIndexes { get; } = [];

        public int DisposedLocks { get; private set; }

        public int HeartbeatCount { get; private set; }

        public void AllowIterations(int count)
        {
            for (var i = 0; i < count; i++)
                _versionOk.Enqueue(true);
        }

        public void RefuseNextLock() => _lockGrants.Enqueue(false);

        public void FailNextHeartbeat() => _heartbeats.Enqueue(false);

        public void FailWrite(Exception error) => _writeError = error;

        public SourceData EnqueuePage(int count, bool full, long? newSeed = null)
        {
            var page = new SourceData
            {
                Count = count,
                Full = full,
                NewSeed = newSeed
            };
            _pages.Enqueue(page);
            return page;
        }

        public IndexWorker CreateWorker() =>
            new(
                Index,
                new Locker(this),
                new SourceFactory(this),
                new DestinationFactory(this),
                new VersionController(this),
                new SeedStore(this));

        sealed class VersionController(Fixture fixture) : IIndexVersionController
        {
            public string CalculateVersion(IndexDescription idxDesc)
            {
                fixture.VersionRequests.Add(idxDesc);
                return fixture.Version;
            }

            public Task<bool> CheckoutVersionAsync(string actualVersion)
            {
                fixture.CheckedOutVersions.Add(actualVersion);
                var ok = fixture._versionOk.Count > 0 && fixture._versionOk.Dequeue();
                return Task.FromResult(ok);
            }
        }

        sealed class Locker(Fixture fixture) : IIndexLocker
        {
            public Task<IIndexLock?> TryLockAsync(string indexId, CancellationToken ct)
            {
                fixture.LockedIndexes.Add(indexId);
                var granted = fixture._lockGrants.Count == 0 || fixture._lockGrants.Dequeue();
                var res = granted ? (IIndexLock)new Lock(fixture) : null;
                return Task.FromResult(res);
            }
        }

        sealed class Lock(Fixture fixture) : IIndexLock
        {
            public Task<bool> HeartbeatAsync(CancellationToken ct)
            {
                fixture.HeartbeatCount++;
                var alive = fixture._heartbeats.Count == 0 || fixture._heartbeats.Dequeue();
                return Task.FromResult(alive);
            }

            public ValueTask DisposeAsync()
            {
                fixture.DisposedLocks++;
                return ValueTask.CompletedTask;
            }
        }

        sealed class SourceFactory(Fixture fixture) : ISourceRepositoryFactory
        {
            public ISourceRepository Create(IndexDescription idxDesc) => new Source(fixture);
        }

        sealed class Source(Fixture fixture) : ISourceRepository
        {
            public Task<SourceData> ReadAsync(long seed, CancellationToken cancellationToken)
            {
                fixture.ReadSeeds.Add(seed);
                if (fixture._pages.Count == 0)
                    throw new InvalidOperationException("ReadAsync was called after the scripted pages ended.");

                return Task.FromResult(fixture._pages.Dequeue());
            }
        }

        sealed class DestinationFactory(Fixture fixture) : IDestinationRepositoryFactory
        {
            public IDestinationRepository Create(IndexDescription idxDesc) => new Destination(fixture);
        }

        sealed class Destination(Fixture fixture) : IDestinationRepository
        {
            public Task WriteAsync(SourceData data)
            {
                if (fixture._writeError != null)
                    throw fixture._writeError;

                fixture.WrittenPages.Add(data);
                return Task.CompletedTask;
            }
        }

        sealed class SeedStore(Fixture fixture) : ISeedStorage
        {
            public Task<long> GetAsync(string idxId)
            {
                fixture.LoadedSeedIndexes.Add(idxId);
                return Task.FromResult(fixture.Seed);
            }

            public Task SetAsync(string idxId, long seed)
            {
                fixture.SavedSeedIndexes.Add(idxId);
                fixture.SavedSeeds.Add(seed);
                fixture.Seed = seed;
                return Task.CompletedTask;
            }
        }
    }
}
