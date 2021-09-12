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
    [CommandHandler(Constants.Command.NotFoundAnswer, "", ServiceLifetime.Singleton, false)]
    public class NotFoundAnswerCommandHandler : ICommandHandler
    {
        private ILogger<NotFoundAnswerCommandHandler> _logger;
        private readonly string _msg;
        private readonly DataFormatter _formatter;

        public NotFoundAnswerCommandHandler(ILogger<NotFoundAnswerCommandHandler> logger, BotSettings settings, DataFormatter formatter)
        {
            _logger = logger;
            _msg = settings.NotFoundAnswerMessage;
            _formatter = formatter;
        }
        
        public async Task Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext, CancellationToken cancellationToken) 
            where TActivity : IActivity
        {
            var formatRequest = turnContext.ToFormatRequest();

            var res = _formatter.Format(_msg, formatRequest);
            
            var msg = MessageFactory.Text(res);
            await turnContext.SendActivityAsync(msg, cancellationToken);   
        }
    }
}