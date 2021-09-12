using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UniversityBot.ASP.Bots;
using UniversityBot.EF.Extension;
using UniversityBot.ServiceProvider.Configuration;
using UniversityBot.Sqlite;

namespace UniversityBot.ASP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private ILoggerFactory LoggerFactory { get; set; }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            services.AddHttpClient();
            
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            
            services.AddSingleton<IStorage, MemoryStorage>();
            services.AddSingleton<ConversationState>();
            services.AddSingleton<UserState>();
            
            services.AddTransient<IBot, QuizBot>();

            var dbFilePath = Configuration["DBFilePath"];
            var connectionString = SqliteContextFactory.BuildConnectionString(dbFilePath);
            var sqlDbFactory = new SqliteContextFactory(connectionString, true);
            using var db = sqlDbFactory.Create(optionConfigurator: null);
            services.ConfigureUniversityBotCore(db, sqlDbFactory, true);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}