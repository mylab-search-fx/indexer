using MyLab.Log;
using MyLab.Search.Indexer.Application;
using MyLab.Search.Indexer.Infrastructure;
using MyLab.Search.Indexer.Worker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddIndexerAppLogic(builder.Configuration)
    .AddIndexerInfrastructure()
    .AddLogging(c => c.AddMyLabConsole())
    .AddHostedService<IndexerBackgroundService>();

var app = builder.Build();

app.Run();
