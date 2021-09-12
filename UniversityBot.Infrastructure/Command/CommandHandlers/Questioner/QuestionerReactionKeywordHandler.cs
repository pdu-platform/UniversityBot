using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniversityBot.Core;
using UniversityBot.Core.BotMessage;
using UniversityBot.Core.Command.Attributes;
using UniversityBot.EF;

namespace UniversityBot.Infrastructure.Command.CommandHandlers.Questioner
{
    [CommandHandler(Constants.Command.KeywordReaction, "", ServiceLifetime.Scoped, false)]
    public class QuestionerReactionKeywordHandler : QuestionerCommandHandlerBase
    {
        public QuestionerReactionKeywordHandler(ILogger<QuestionerReactionKeywordHandler> logger, AppDbContext appDb, IMemoryCache cache, FileTransformer fileTransformer) 
            : base(logger, appDb, cache, fileTransformer)
        {
        }

        public override async Task Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext, CancellationToken cancellationToken)
        {
            if (request.Message is not BotReactionKeywordMarker mrk)
                return;

            var result = mrk
                .BotMessageIds
                .Select(async id =>
                {
                    var answer = Filter(e => e.Id == id).Select(e => e.Answer).FirstOrDefaultAsync(cancellationToken: cancellationToken);
                    var child = AllChildFor(id, cancellationToken);
                    var attachment = SelectAttachment(id, cancellationToken);
                    return new CurrentDisplayQuestioner(await answer, await attachment, await child);
                })
                .ToList();

            var unwrapResult = await Task.WhenAll(result);

            var actions = new List<CardAction>();
            foreach (var questioner in unwrapResult)
                await HandleCore(questioner, actions, turnContext, cancellationToken);
        }
    }
}