using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Application.Abstraction;

public interface IDestinationRepositoryFactory
{
    IDestinationRepository Create(IndexDescription idxDesc);
}