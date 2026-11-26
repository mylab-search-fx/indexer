using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Application.Data;
using MyLab.Search.Indexer.Infrastructure.Database.SourceRepository;

namespace Infrastructure.Database.IntegrationTests.SourceRepository;

public partial class DbSourceRepositoryBehavior
{
    [Fact(Timeout = 10000)]
    public Task ShouldProvideStartDataWithIntSeed()
    {
        var expected = _items.Take(2);
        return ShouldProvideDataWithIntSeed
            (
                0, 
                expected, 
                true
            );
    }

    [Fact(Timeout = 10000)]
    public Task ShouldProvideMiddleDataWithIntSeed()
    {
        var expected = _items.Skip(2).Take(2);
        return ShouldProvideDataWithIntSeed
        (
            1, 
            expected, 
            true
        );
    }

    [Fact(Timeout = 10000)]
    public Task ShouldProvideEndDataWithIntSeed()
    {
        var expected = _items.TakeLast(1);
        return ShouldProvideDataWithIntSeed
            (
                _items.Length-2, 
                expected, 
                false
            );
    }

    [Fact(Timeout = 10000)]
    public Task ShouldProvideStartDataWithDateTimeSeed()
    {
        var expected = _items.Take(2);
        return ShouldProvideDataWithDateTimeSeed
        ( 
            0, 
            expected, 
            true
        );
    }

    [Fact(Timeout = 10000)]
    public Task ShouldProvideMiddleDataWithDateTimeSeed()
    {
        var expected = _items.Skip(2).Take(2);
        return ShouldProvideDataWithDateTimeSeed
        (
            DateTimeToIntConverter.DateTimeToLong(_items[1].LastModified), 
            expected, 
            true
        );
    }

    [Fact(Timeout = 10000)]
    public Task ShouldProvideEndDataWithDateTimeSeed()
    {
        var expected = _items.TakeLast(1);
        return ShouldProvideDataWithDateTimeSeed
        (
            DateTimeToIntConverter.DateTimeToLong(_items[^2].LastModified), 
            expected, 
            false
        );
    }

    async Task ShouldProvideDataWithIntSeed(long seed, IEnumerable<TestDbItem> expectedFoundEnum, bool expectedFull)
    {
        // Arrange
        var cfg = new IndexConfig(PageSize: PageSize);
        var repository = new DbSourceRepository(DbTestTools.CreateIntSeedIndex(cfg), _srcDb);

        var expectedFound = expectedFoundEnum.ToArray();
        var maxSeed = expectedFound.Select(itm => itm.Id).Max();

        // Act
        var data = await repository.ReadAsync(seed, CancellationToken.None);

        // Assert
        Assert.NotNull(data);
        Assert.Equal(expectedFound.Length, data.Count);
        Assert.Equal(expectedFound.Length, data.Items.Length);
        Assert.Equal(expectedFull, data.Full);
        Assert.Equal(maxSeed, data.NewSeed);
        Assert.All(data.Items, (itm, i) =>
        {
            Assert.Equal(expectedFound[i].Id, itm.Seed);
            Assert.Equal(SourceDataValueKind.Number, itm.Id.Kind);
            Assert.Equal(expectedFound[i].Id, (int)itm.Id.Value);
            Assert.Equal(SourceDataValueKind.String, itm.Fields["val"].Kind);
            Assert.Equal(expectedFound[i].Value, itm.Fields["val"].Value);
        });
    }

    async Task ShouldProvideDataWithDateTimeSeed(long seed, IEnumerable<TestDbItem> expectedFoundEnum, bool expectedFull)
    {
        // Arrange
        var cfg = new IndexConfig(PageSize: PageSize);
        var repository = new DbSourceRepository(DbTestTools.CreateDtSeedIndex(cfg), _srcDb);

        var expectedFound = expectedFoundEnum.ToArray();
        var maxSeed = expectedFound
            .Select(itm => DtToLongAndRound(itm.LastModified))
            .Max();

        // Act
        var data = await repository.ReadAsync(seed, CancellationToken.None);

        // Assert
        Assert.NotNull(data);
        Assert.Equal(expectedFound.Length, data.Count);
        Assert.Equal(expectedFound.Length, data.Items.Length);
        Assert.Equal(expectedFull, data.Full);
        Assert.Equal(maxSeed, data.NewSeed);
        Assert.All(data.Items, (itm, i) =>
        {
            Assert.Equal(DtToLongAndRound(expectedFound[i].LastModified), itm.Seed);
            Assert.Equal(SourceDataValueKind.Number, itm.Id.Kind);
            Assert.Equal(expectedFound[i].Id, (int)itm.Id.Value);
            Assert.Equal(SourceDataValueKind.String, itm.Fields["val"].Kind);
            Assert.Equal(expectedFound[i].Value, itm.Fields["val"].Value);
        });
    }
}
