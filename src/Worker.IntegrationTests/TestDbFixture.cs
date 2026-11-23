using MyLab.DbTest;
using MyLab.Search.Indexer.Infrastructure.Database;

namespace Worker.IntegrationTests;

public class TestDbFixture : RemoteDbFixture
{
    const string ConnectionString =
        "Server=127.0.0.1;Port=13306;User ID=test;Password=test;SslMode=None;AllowPublicKeyRetrieval=True";

    public TestDbFixture() 
        : base(new MariaDb10DataProvider(), ConnectionString)
    {
    }

}