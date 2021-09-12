using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniversityBot.Core;
using UniversityBot.Core.Command.Attributes;
using UniversityBot.Core.DAL;
using UniversityBot.Core.Util;
using UniversityBot.EF;
using UniversityBot.Infrastructure.Extension;

namespace UniversityBot.Infrastructure.Command.CommandHandlers
{
    [CommandHandler(Constants.Command.Hello, "Привет", ServiceLifetime.Scoped, false)]
    public class WelcomeCommandHandler : ICommandHandler
    {
        private ILogger<WelcomeCommandHandler> _logger;
        private DataFormatter _formatter;
        private AppDbContext _appDb;
        private bool _splitWelcomeMessage;

        public WelcomeCommandHandler(ILogger<WelcomeCommandHandler> logger, AppDbContext appDb, BotSettings settings, DataFormatter formatter)
        {
            _logger = logger;
            _appDb = appDb;
            _formatter = formatter;
            _splitWelcomeMessage = settings.SplitWelcomeMessage;
        }
        
        public async Task Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext, CancellationToken cancellationToken) 
            where TActivity : IActivity
        {
            var items = await _appDb
                .Welcome
                .AsNoTracking()
                .OrderBy(e => e.Order)
                .Select(e => e.Text)
                .ToListAsync(cancellationToken: cancellationToken);

            var formatRequest = turnContext.ToFormatRequest();

            var formatData = items.Select(e => _formatter.Format(e, formatRequest));
            
            if (_splitWelcomeMessage)
            {
                var activities = formatData.Select(t => (IActivity)MessageFactory.Text(t)).ToArray(items.Count);
                await turnContext.SendActivitiesAsync(activities, cancellationToken);
            }
            else
            {
                var txt = formatData.JoinToString(Environment.NewLine);
                var msg = MessageFactory.Text(txt);
                await turnContext.SendActivityAsync(msg, cancellationToken);   
            }
        }
    }
}