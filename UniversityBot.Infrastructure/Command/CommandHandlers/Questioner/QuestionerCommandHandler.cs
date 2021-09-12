using System;
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
using UniversityBot.Core.Enums;
using UniversityBot.EF;

namespace UniversityBot.Infrastructure.Command.CommandHandlers.Questioner
{
    [CommandHandler(new []{ Constants.Command.QuestionWithoutSlash, Constants.Command.Question}, "Вопросы", ServiceLifetime.Scoped)]
    public class QuestionerCommandHandler : QuestionerCommandHandlerBase
    {
        public QuestionerCommandHandler(ILogger<QuestionerCommandHandler> logger, AppDbContext appDb, IMemoryCache cache, FileTransformer fileTransformer)
            : base(logger, appDb, cache, fileTransformer)
        {
        }
        
        public override async Task Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext, CancellationToken cancellationToken)
        {
            Guid? parentMsgId = null;
            var imageAttachments = new List<Attachment>();
            string answer = null;
            
            if (request.Message is BotMessageMarker bmm)
            {
                parentMsgId = bmm.Id;
                
                if (bmm.Type == BotMessageType.QuestionBack)
                {
                    parentMsgId = await TryFindParentId(bmm.Id, cancellationToken);
                }
                else
                {
                    imageAttachments = await SelectAttachment(bmm.Id, cancellationToken);
                    answer = await Filter(q => q.Id == bmm.Id)
                        .Select(e => e.Answer).
                        FirstOrDefaultAsync(cancellationToken: cancellationToken);
                }
            }
            
            var child = await AllChildFor(parentMsgId, cancellationToken);

            var actions = new List<CardAction>();
            
            if (parentMsgId.HasValue)
            {
                var card = CreateCardAction("Назад", BotMessageMarker.BackQuestion(parentMsgId.Value));
                actions.Add(card);
            }
            
            var questioner = new CurrentDisplayQuestioner(answer, imageAttachments, child);
            await HandleCore(questioner, actions, turnContext, cancellationToken);
        }
        
        private Task<Guid?> TryFindParentId(Guid id, CancellationToken cancellationToken)
        {
            return AppDb.Questioner
                .AsNoTracking()
                .Where(e => e.Id == id && e.ParentId.HasValue)
                .Select(e => e.ParentId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
    }
}