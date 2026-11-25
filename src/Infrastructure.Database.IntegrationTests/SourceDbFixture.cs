using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.Database;

namespace Infrastructure.Database.IntegrationTests;

public class SourceDbFixture : RemoteDbFixture
{
    public SourceDbFixture() :
        base
        (
            new MariaDb10DataProvider(),
            DbTestTools.CreateCs(DbTestTools.SourceDbname)
        )
    {

    }
}