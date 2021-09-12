using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniversityBot.Core.Command.Attributes;
using UniversityBot.EF;
using UniversityBot.Infrastructure.Extension;

namespace UniversityBot.Infrastructure.Command.CommandHandlers
{
    [IgnoreCommandHandler]
    public class DatabaseCommandHandler : ICommandHandler
    {
        private readonly IServiceProvider _provider;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DatabaseCommandHandler> _logger;
        
        public DatabaseCommandHandler(IServiceProvider provider, IMemoryCache cache, ILogger<DatabaseCommandHandler> logger)
        {
            _provider = provider;
            _cache = cache;
            _logger = logger;
        }
        
        public async Task Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext, CancellationToken cancellationToken) where TActivity : IActivity
        {
            var command = request.Command;
            if (!_cache.TryGetValue(command, out IActivity activity))
            {
                using var scope = _provider.CreateScope();
                
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var botCommand = await db
                    .CommandHandleNames
                    .AsNoTracking()
                    .Include(e => e.BotCommand)
                        .ThenInclude(e => e.Questions)
                    
                    .Where(e => e.Question == command)
                    .Select(e => e.BotCommand)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (botCommand == null)
                {
                    _logger.LogError("Not found handler for '{0}'", command);
                    return;
                }

                var handleNames = botCommand.Questions.Select(e => e.Question).ToList();
                var activityResult = botCommand.ToActivity();
                
                if (activityResult.IsFail)
                {
                    _logger.LogError("Can't convert BotCommand = {0} to Activity. {1}", botCommand.Id, activityResult.Error);
                    activity = MessageFactory.Text("Что-то пошло не так... ⛔");
                }
                else
                {
                    activity = activityResult.Value;
                }
                
                foreach (var handleName in handleNames)
                    _cache.Set(handleName, activity);
            }

            await turnContext.SendActivityAsync(activity, cancellationToken);
        }
    }
}