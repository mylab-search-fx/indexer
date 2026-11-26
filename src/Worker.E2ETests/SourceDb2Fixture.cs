using Infrastructure.Database.IntegrationTests;
using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.Database;

namespace Worker.E2ETests;

public class SourceDb2Fixture : RemoteDbFixture
{
    public SourceDb2Fixture() :
        base
        (
            new MariaDb10DataProvider(),
            DbTestTools.CreateCs(E2ETestTools.Source2DbName)
        )
    {

    }
}