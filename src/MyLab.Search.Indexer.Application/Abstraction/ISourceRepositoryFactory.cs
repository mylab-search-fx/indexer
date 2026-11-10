using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Application.Abstraction;

public interface ISourceRepositoryFactory
{
    ISourceRepository Create(IndexDescription idxDesc);
}