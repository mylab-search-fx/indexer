using MyLab.Search.Indexer.Application.Abstraction;
using MyLab.Search.Indexer.Application.Indexing;
using MyLab.Search.Indexer.Infrastructure.FileSystem;
using MyLab.Search.Indexer.Worker.Options;
using MyLab.Search.Indexer.Worker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<IIndexWorkerOrchestrator, IIndexWorkerOrchestrator>()
    .AddSingleton<IIndexWorkerFactory, IndexWorkerFactory>()
    .AddSingleton<IIndexConfigProvider, LocalFileIndexConfigProvider>()
    .Configure<WorkerOptions>(builder.Configuration.GetSection(WorkerOptions.SectionName))
    .AddHostedService<IndexerBackgroundService>();

var app = builder.Build();

app.Run();
