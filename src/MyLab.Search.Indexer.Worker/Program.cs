using MyLab.Search.Indexer.Worker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<IndexerBackgroundService>();

var app = builder.Build();

app.Run();
