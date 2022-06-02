﻿using System;
using System.Threading.Tasks;
using LinqToDB.Async;
using MyLab.DbTest;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Xunit;

namespace UnitTests
{
    public partial class DataSourceServiceBehavior : IClassFixture<TmpDbFixture<DataSourceServiceBehavior.DbInitializer>>
    {
        [Fact]
        public async Task ShouldNotLoadFromStreamIfEmpty()
        {
            //Arrange
            var dbMgr = await _dbFxt.CreateDbAsync();

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Stream,
                SyncDbQuery = "select id, content from entities where id > @seed"
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Empty(loads);
        }

        [Fact]
        public async Task ShouldNotLoadFromHeapIfEmpty()
        {
            //Arrange
            var dbMgr = await _dbFxt.CreateDbAsync();
            var seedSrv = new TestSeedService();
            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Heap,
                SyncDbQuery = "select id, content from entities where last_change_dt > @seed"
            };
            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Empty(loads);
        }

        [Fact]
        public async Task ShouldLoadAllDataFromStream()
        {
            //Arrange

            var ent0 = new TestEntity { Id = 0, Content = "0-content" };
            var ent1 = new TestEntity { Id = 1, Content = "1-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Stream,
                SyncDbQuery = "select id, content from entities where id > @seed limit @offset, @limit",
                SyncPageSize = 1
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Equal(2, loads.Length);
            Assert.Single(loads[0].Batch.Entities);
            Assert.Equal("0", loads[0].Batch.Entities[0].Id);

            AssertEntity(ent0, loads[0].Batch.Entities[0].Entity);
            AssertEntity(ent1, loads[1].Batch.Entities[0].Entity);
        }

        [Fact]
        public async Task ShouldLoadAllDataFromHeap()
        {
            //Arrange

            var ent0 = new TestEntity { Id = 0, Content = "0-content", LastChangeDt = DateTime.Now };
            var ent1 = new TestEntity { Id = 1, Content = "1-content", LastChangeDt = DateTime.Now };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Heap,
                SyncDbQuery = "select id, content from entities where last_change_dt > @seed limit @offset, @limit",
                SyncPageSize = 1
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Equal(2, loads.Length);
            Assert.Single(loads[0].Batch.Entities);
            Assert.Equal("0", loads[0].Batch.Entities[0].Id);

            AssertEntity(ent0, loads[0].Batch.Entities[0].Entity);
            AssertEntity(ent1, loads[1].Batch.Entities[0].Entity);
        }

        [Fact]
        public async Task ShouldLoadDeltaFromStream()
        {
            //Arrange
            const long lastProcessedEntityId = 0;

            var ent0 = new TestEntity { Id = lastProcessedEntityId, Content = "0-content" };
            var ent1 = new TestEntity { Id = 1, Content = "1-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            await seedSrv.SaveSeedAsync("foo-index", lastProcessedEntityId);

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Stream,
                SyncDbQuery = "select id, content from entities where id > @seed limit @offset, @limit",
                SyncPageSize = 1
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Single(loads);
            Assert.Single(loads[0].Batch.Entities);
            Assert.Equal("1", loads[0].Batch.Entities[0].Id);
            AssertEntity(ent1, loads[0].Batch.Entities[0].Entity);
        }

        [Fact]
        public async Task ShouldLoadDeltaFromHeap()
        {
            //Arrange
            var lastIndexedDt = DateTime.Now;

            var ent0 = new TestEntity { Id = 0, Content = "0-content", LastChangeDt = lastIndexedDt.AddMinutes(-1) };
            var ent1 = new TestEntity { Id = 1, Content = "1-content", LastChangeDt = lastIndexedDt.AddMinutes(1) };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            await seedSrv.SaveSeedAsync("foo-index", lastIndexedDt);

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Heap,
                SyncDbQuery = "select id, content from entities where last_change_dt > @seed limit @offset, @limit",
                SyncPageSize = 1
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Single(loads);
            Assert.Single(loads[0].Batch.Entities);
            Assert.Equal("1", loads[0].Batch.Entities[0].Id);
            AssertEntity(ent1, loads[0].Batch.Entities[0].Entity);
        }
    }

    
}
