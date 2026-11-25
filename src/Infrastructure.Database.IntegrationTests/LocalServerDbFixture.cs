using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.Database;

namespace Infrastructure.Database.IntegrationTests;

public class LocalServerDbFixture : RemoteDbFixture
{
    public LocalServerDbFixture()
        : base(new MariaDb10DataProvider(), DbTestTools.CreateCs())
    {
    }
}