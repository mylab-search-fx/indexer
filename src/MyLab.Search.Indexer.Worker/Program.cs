using MyLab.Log;
using MyLab.Search.Indexer.Application;
using MyLab.Search.Indexer.Worker.Services;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services
    .AddIndexerAppLogic()
    .AddRedisInfrastructure()
    .AddFileSystemInfrastructure()
    .AddDatabaseInfrastructure()
    .AddOpenSearchInfrastructure()
    .AddLogging(c => c.AddMyLabConsole())
    .AddHostedService<IndexerBackgroundService>()
    
    .ConfigureIndexerAppLogic(cfg)
    .ConfigureDatabaseInfrastructure(cfg)
    .ConfigureFileSystemInfrastructure(cfg)
    .ConfigureOpenSearchInfrastructure(cfg)
    .ConfigureRedisInfrastructure(cfg);

var app = builder.Build();

app.Run();
