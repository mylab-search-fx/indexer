using MyLab.Search.Indexer.Application.Data;

namespace MyLab.Search.Indexer.Application.Abstraction;

public interface IDestinationRepository
{
    Task WriteAsync(SourceData data);
}