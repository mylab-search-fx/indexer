using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Infrastructure.DB;

class DBIndexVersionController  : IIndexVersionController
{
    public string CalculateVersion(IndexDescription idxDesc)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckoutVersionAsync(string actualVersion)
    {
        throw new NotImplementedException();
    }
}