using MyLab.Search.Indexer.Application;
using MyLab.Search.Indexer.Infrastructure;
using MyLab.Search.Indexer.Worker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddIndexerAppLogic(builder.Configuration)
    .AddIndexerInfrastructure()
    .AddHostedService<IndexerBackgroundService>();

var app = builder.Build();

app.Run();
