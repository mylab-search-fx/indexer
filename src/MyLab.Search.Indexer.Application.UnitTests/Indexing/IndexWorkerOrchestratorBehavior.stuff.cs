using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Application.Data;

namespace MyLab.Search.Indexer.Application.UnitTests.Indexing;

public partial class IndexWorkerOrchestratorBehavior
{
    sealed class OverlapGate(int workerCount) : IIndexLocker
    {
        int _inFlight;
        int _maxInFlight;
        readonly TaskCompletionSource _allEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task AllEntered => _allEntered.Task;

        public int MaxInFlight => Volatile.Read(ref _maxInFlight);

        public async Task<IIndexLock?> TryLockAsync(string indexId, CancellationToken ct)
        {
            var current = Interlocked.Increment(ref _inFlight);

            int priorPeak;
            while ((priorPeak = Volatile.Read(ref _maxInFlight)) < current)
            {
                if (Interlocked.CompareExchange(ref _maxInFlight, current, priorPeak) == priorPeak)
                    break;
            }

            if (current == workerCount)
                _allEntered.TrySetResult();

            await WaitUntilCancelledAsync(ct);

            return null;
        }

        async Task WaitUntilCancelledAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            finally
            {
                Interlocked.Decrement(ref _inFlight);
            }

            return;
        }
    }

    sealed class WorkerServices(OverlapGate gate) : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IIndexLocker))
                return gate;
            if (serviceType == typeof(ISourceRepositoryFactory))
                return IdleSourceFactory.Instance;
            if (serviceType == typeof(IDestinationRepositoryFactory))
                return IdleDestinationFactory.Instance;
            if (serviceType == typeof(IIndexVersionController))
                return PassingVersionController.Instance;
            if (serviceType == typeof(ISeedStorage))
                return IdleSeedStorage.Instance;

            return null;
        }
    }

    sealed class FixedIndexDescProvider(IReadOnlyList<IndexDescription> indexes) : IIndexDescProvider
    {
        public Task<IReadOnlyList<IndexDescription>> ProvideAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult(indexes);
    }

    sealed class PassingVersionController : IIndexVersionController
    {
        public static readonly PassingVersionController Instance = new();

        public string CalculateVersion(IndexDescription idxDesc) => "v1";

        public Task<bool> CheckoutVersionAsync(string actualVersion) => Task.FromResult(true);
    }

    sealed class IdleSourceFactory : ISourceRepositoryFactory
    {
        public static readonly IdleSourceFactory Instance = new();

        public ISourceRepository Create(IndexDescription idxDesc) => IdleSource.Instance;
    }

    sealed class IdleSource : ISourceRepository
    {
        public static readonly IdleSource Instance = new();

        public Task<SourceData> ReadAsync(long seed, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("ReadAsync is outside this test.");
    }

    sealed class IdleDestinationFactory : IDestinationRepositoryFactory
    {
        public static readonly IdleDestinationFactory Instance = new();

        public IDestinationRepository Create(IndexDescription idxDesc) => IdleDestination.Instance;
    }

    sealed class IdleDestination : IDestinationRepository
    {
        public static readonly IdleDestination Instance = new();

        public Task WriteAsync(SourceData data) =>
            throw new InvalidOperationException("WriteAsync is outside this test.");
    }

    sealed class IdleSeedStorage : ISeedStorage
    {
        public static readonly IdleSeedStorage Instance = new();

        public Task<long> GetAsync(string idxId) =>
            throw new InvalidOperationException("GetAsync is outside this test.");

        public Task SetAsync(string idxId, long seed) =>
            throw new InvalidOperationException("SetAsync is outside this test.");
    }
}
