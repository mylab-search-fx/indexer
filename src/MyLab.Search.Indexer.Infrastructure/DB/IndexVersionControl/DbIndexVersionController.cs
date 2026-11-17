using System.Security.Cryptography;
using System.Text;
using LinqToDB;
using LinqToDB.Async;
using MyLab.Db;
using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Config;

namespace MyLab.Search.Indexer.Infrastructure.DB.IndexVersionControl;

class DbIndexVersionController(IDbManager dbMgr)  : IIndexVersionController
{
    public string CalculateVersion(IndexDescription idxDesc)
    {
        var scriptBin = Encoding.UTF8.GetBytes(idxDesc.SqlQuery);
        var hash = MD5.HashData(scriptBin);
        return Convert.ToHexStringLower(hash).Replace("-", "");
    }

    public async Task<bool> CheckoutVersionAsync(string actualVersion, CancellationToken ct)
    {
        await using var db = dbMgr.Use();

        bool exists = await db
            .Tab<IndexVersion>()
            .AnyAsync(v => v.Value == actualVersion, token: ct);

        if (!exists)
        {
            var inserted = await db
                .Tab<IndexVersion>()
                .InsertAsync(() => new IndexVersion
                {
                    Registered = DateTime.Now,
                    Value = actualVersion
                }, token: ct);

            return inserted != 0;
        }

        var latestVersion = await db
            .Tab<IndexVersion>()
            .OrderByDescending(v => v.Registered)
            .Select(v => v.Value)
            .FirstAsync(token: ct);

        return latestVersion == actualVersion;
    }
}