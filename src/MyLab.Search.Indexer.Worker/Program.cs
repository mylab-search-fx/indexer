var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWorker()
    .ConfigureWorker(builder.Configuration);

var app = builder.Build();

app.Run();
