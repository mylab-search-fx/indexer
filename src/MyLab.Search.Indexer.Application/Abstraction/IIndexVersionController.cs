using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Application.Abstraction;

public interface IIndexVersionController
{
    string CalculateVersion(IndexDescription idxDesc);
    
    Task<bool> CheckoutVersionAsync(string actualVersion);
}