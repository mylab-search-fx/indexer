using System.Runtime.Versioning;
using System.Text.Json;
using LinqToDB;
using LinqToDB.Data;
using MyLab.Db;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Application.Data;

namespace MyLab.Search.Indexer.Infrastructure.Database.SourceRepository;

class DbSourceRepository(IndexDescription idxDesc, IDbManager db) : ISourceRepository
{
    public async Task<SourceData> ReadAsync(long seed, CancellationToken cancellationToken)
    {
        var parameters = new[]
        {
            new DataParameter
            (
                "seed",
                seed == 0 ? -1 : seed,
                DataType.Long
            ),
            new DataParameter
            (
                "seed_dt",
                DateTimeToIntConverter.LongToDateTime(seed),
                DataType.DateTime
            ),
            new("limit", idxDesc.Config.PageSize, DataType.Int32)
        };

        await using var reader = await db.DoOnce().ExecuteReaderAsync(idxDesc.SqlQuery, parameters, cancellationToken);

        var items = await ReadItemsAsync(cancellationToken, reader);

        long newSeed;

        if (items.Length == 0)
        {
            newSeed = seed;
        }
        else
        {
            newSeed = items
                .Select(itm => itm.Seed)
                .Max(s => s);
        }

        return new SourceData
        {
            Items = items.ToArray(),
            Count = items.Length,
            Full = items.Length == idxDesc.Config.PageSize,
            NewSeed = newSeed
        };
    }

    static async Task<SourceDataItem[]> ReadItemsAsync(CancellationToken cancellationToken, DataReaderAsync reader)
    {
        var items = new List<SourceDataItem>();
        await foreach (var item in reader.QueryToAsyncEnumerable(SourceDataItemReader.ReadItem).WithCancellation(cancellationToken))
        {
            items.Add(item);
        }

        return items.ToArray();
    }
}