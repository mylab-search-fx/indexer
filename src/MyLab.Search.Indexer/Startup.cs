using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyLab.Db;
using MyLab.HttpMetrics;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using MyLab.StatusProvider;
using MyLab.TaskApp;
using MyLab.WebErrors;
using Prometheus;

namespace MyLab.Search.Indexer
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of <see cref="Startup"/>
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton(_configuration)
                .AddTaskLogic<IndexerTaskLogic>()
                .AddAppStatusProviding()
                .AddDbTools<ConfiguredDataProviderSource>(_configuration)
                .AddEsTools(_configuration, "ES")
                .AddLogging(l => l.AddConsole())
                .AddSingleton<IIndexResourceProvider, IndexResourceProvider>()
                .AddSingleton<ISeedService, FileSeedService>()
                .AddSingleton<IDataIndexer, DataIndexer>()
                .AddSingleton<IDataSourceService, DbDataSourceService>()
                .AddSingleton<IIndexMappingService, IndexMappingService>()
                .AddSingleton<IPushIndexer, PushIndexer>()
                .AddSingleton<IKickIndexer, KickIndexer>()
                .AddRabbit()
                .AddRabbitConsumers<IndexerConsumerRegistrar>()
                .AddUrlBasedHttpMetrics()
                .AddControllers(c => c.AddExceptionProcessing());

            services
                .Configure<IndexerOptions>(_configuration.GetSection("Indexer"))
                .Configure<IndexerDbOptions>(_configuration.GetSection("DB"))
                .ConfigureRabbit(_configuration)
                .Configure<ExceptionProcessingOptions>(o => o.HideError =
#if DEBUG
                false
#else
                true
#endif
                );

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting()
                .UseHttpMetrics()
                .UseTaskApi()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapMetrics();
                })
                .UseStatusApi();

        }
    }
}
