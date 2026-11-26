using LinqToDB;
using LinqToDB.DataProvider.MySql;

namespace MyLab.Search.Indexer.Infrastructure.Database;

public class MariaDb10DataProvider : LinqToDB.Internal.DataProvider.MySql.MySqlDataProvider
{
    public MariaDb10DataProvider() : base(
        ProviderName.MariaDB10MySqlConnector,
        MySqlVersion.MariaDB10,
        MySqlProvider.MySqlConnector)
    {
    }
}