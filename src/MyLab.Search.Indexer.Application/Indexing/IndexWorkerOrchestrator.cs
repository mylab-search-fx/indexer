using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Application.Abstraction;

namespace MyLab.Search.Indexer.Application.Indexing
{
    class IndexWorkerOrchestrator(IIndexWorkerFactory factory, IDslLogger? log = null) : IIndexWorkerOrchestrator
    {
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var workers = await factory.CreateAllAsync(cancellationToken) 
                          ?? throw new NullReferenceException("factory.CreateAll()");

            await Task.WhenAll(workers.Select(w => RunAsync(w, cancellationToken)));
        }

        async Task RunAsync(IIndexWorker worker, CancellationToken cancellationToken)
        {
            IDisposable? logScope = null;
            
            if (log != null)
            {
                var scopedFacts = new Dictionary<string, object>
                {
                    { "index", worker.Name }
                };

                logScope = log.BeginScope(scopedFacts);
            }

            log?.Action("Index worker iteration started").Write();

            try
            {
                await worker.RunAsync(cancellationToken);

                log?.Action("Index worker iteration end").Write();
            }
            catch (Exception e)
            {
                log?.Error("Index worker iteration error", e).Write();
            }
            finally
            {
                logScope?.Dispose();
            }
        }
    }
}
