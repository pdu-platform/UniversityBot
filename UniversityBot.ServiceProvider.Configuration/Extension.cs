using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using UniversityBot.Core.DAL;
using UniversityBot.Core.Statistic;
using UniversityBot.EF;
using UniversityBot.EF.Repository;
using UniversityBot.EF.Statistic;
using UniversityBot.Infrastructure;
using UniversityBot.Infrastructure.Command;
using UniversityBot.Infrastructure.Extension;
using UniversityBot.Infrastructure.WordProcessing;

namespace UniversityBot.ServiceProvider.Configuration
{
    public static class Extension
    {
        public static IServiceCollection ConfigureUniversityBotCore<TFactory>(this IServiceCollection self, AppDbContext appDb, TFactory contextFactory, bool includeCommandFromDatabase)
            where TFactory : IContextFactory
        {
            var commandHandlerMetadataStore = CommandHandlerMetadataStore.Create(appDb, includeCommandFromDatabase);
            
            self.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            self.TryAddSingleton(sp =>
            {
                var provider = sp.GetRequiredService<ObjectPoolProvider>();
                return provider.CreateStringBuilderPool();
            });
            
            return self
                .AddScoped<UnitOfWork>()
                
                .AddSingleton<KeywordAnalyzer>()
                .AddSingleton<WordLemmatizer>()
                .AddSingleton<HashService>()
                .AddSingleton<KeywordFactory>()
                
                .AddSingleton<FileTransformer>()
                .AddDbContextPool<AppDbContext>(builder => contextFactory.Create(builder))
                .AddTransient<INotFoundWordStore, NotFoundWordStore>()
                .AddSingleton(sp =>
                {
                    using var scope = sp.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var settings = context.Settings.AsNoTracking().FirstOrDefault();
                    if (settings == null)
                    {
                        settings = BotSettings.CreateDefault(Guid.Empty);
                        context.Settings.Add(settings);
                        context.SaveChanges();
                        context.Entry(settings).State = EntityState.Detached;
                    }

                    return settings;
                })
                .AddMemoryCache()
                .AddSingleton(commandHandlerMetadataStore)
                .AddCommandHandlers(commandHandlerMetadataStore)
                .AddSingleton<CommandRouter>()
                
                .AddSingleton<DataFormatter>()
                
                .AddScoped<AttachmentRepository>()
                .AddScoped<CommandRepository>()
                .AddScoped<NotFoundWordRepository>()
                .AddScoped<QuestionRepository>()
                .AddScoped<ReactionKeywordRepository>()
                .AddScoped<WelcomeRepository>();
        }
    }
}