using Microsoft.Extensions.Logging;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Application.Data;
using MyLab.Search.Indexer.Application.Tools;

namespace MyLab.Search.Indexer.Application.Indexing;

public class IndexWorker
    (
        IndexDescription idxDesc,
        IIndexLocker idxLocker,
        ISourceRepositoryFactory srcRepoFactory,
        IDestinationRepositoryFactory destRepoFactory,
        IIndexVersionController idxVerCtrlr,
        ISeedStorage seedStorage,
        ILogger<IndexWorker>? logger = null
    )
{
    readonly IDslLogger? _log = logger?.Dsl();
    
    public string Name => idxDesc.Name;
    
    public async Task RunAsync(CancellationToken ct)
    {
        var srcRepo = srcRepoFactory.Create(idxDesc);
        var destRepo = destRepoFactory.Create(idxDesc);
        var idxVer = idxVerCtrlr.CalculateVersion(idxDesc);

        while (!ct.IsCancellationRequested)
        {
            var idxVersionOk = await idxVerCtrlr.CheckoutVersionAsync(idxVer);

            if (!idxVersionOk)
            {
                _log?.Warning("New version detected").Write();
                break;
            }
            
            await PerformIterationAsync(srcRepo, destRepo, ct);

            var idleTimeSpan = TimeSpan.FromSeconds(idxDesc.Config.Idle);

            _log?.Debug("Iteration idle... ")
                .AndFactIs("time-span", idleTimeSpan)
                .Write();

            await Task.Delay(idleTimeSpan, ct);
        }
    }

    async Task PerformIterationAsync(ISourceRepository srcRepo, IDestinationRepository destRepo, CancellationToken ct)
    {
        var idxLock = await idxLocker.TryLockAsync(idxDesc.Name, ct);
        if (idxLock != null)
        {
            var workLog = _log != null ? IndexingWorkingLog.Start(_log) : null;

            await using (idxLock!)
            {
                bool yetLocked;
                var seed = await seedStorage.GetAsync(idxDesc.Name);
                SourceData readData;

                do
                {
                    readData = await srcRepo.ReadAsync(seed, ct);

                    if (readData.NewSeed.HasValue)
                    {
                        seed = readData.NewSeed.Value;
                        await seedStorage.SetAsync(idxDesc.Name, readData.NewSeed.Value);
                    }

                    workLog?.ReportNextPage(readData.Count);

                    await destRepo.WriteAsync(readData);
                    
                    yetLocked = await idxLock!.HeartbeatAsync(ct);
                } while (readData.Full && yetLocked && !ct.IsCancellationRequested);
            }
            
            workLog?.StopAndWriteLog();
        }
        else
        {
            _log?.Action("Index is busy").Write();
        }
    }
    
}