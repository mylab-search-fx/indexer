using Moq;
using MyLab.Db;
using MyLab.Search.Indexer.Application.Config;
using MyLab.Search.Indexer.Infrastructure.Database.IndexVersionControl;

namespace Infrastructure.UnitTests.DB.IndexVersionControl;

public class DbIndexVersionControllerBehavior
{
    [Fact]
    public void ShouldCalculateIndexVersion()
    {
        //Arrange
        var dbMgrMock = new Mock<IDbManager>();
        var controller = new DbIndexVersionController(dbMgrMock.Object);
        var script = "Some script here";
        var indexDesc = new IndexDescription("foo", script, new IndexConfig());

        //Act
        var ver = controller.CalculateVersion(indexDesc);

        //Assert
        Assert.Equal("5b28b1152abb36394437d71ceab0dc25", ver);
    }
}