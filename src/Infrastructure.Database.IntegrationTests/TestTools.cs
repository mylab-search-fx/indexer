namespace Infrastructure.Database.IntegrationTests;

static class TestTools
{
    public static string CreateCs(string? database = null)
    {
        const string cs = "Server=127.0.0.1;Port=13306;User ID=test;Password=test;SslMode=None;AllowPublicKeyRetrieval=True";
        return database != null
            ? cs + ";Database=" + database
            : cs;
    }
}
