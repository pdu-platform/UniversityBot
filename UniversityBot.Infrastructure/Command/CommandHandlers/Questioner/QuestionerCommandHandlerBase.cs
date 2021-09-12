using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Blowin.Result;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UniversityBot.Core;
using UniversityBot.Core.BotMessage;
using UniversityBot.Core.DAL;
using UniversityBot.Core.Util;
using UniversityBot.EF;

namespace UniversityBot.Infrastructure.Command.CommandHandlers.Questioner
{
    public abstract class QuestionerCommandHandlerBase : ICommandHandler
    {
        protected readonly ILogger Logger;
        protected readonly IMemoryCache Cache;
        protected readonly AppDbContext AppDb;
        protected readonly FileTransformer FileTransformer;
        
        public QuestionerCommandHandlerBase(ILogger logger, AppDbContext appDb, IMemoryCache cache, FileTransformer fileTransformer)
        {
            Logger = logger;
            AppDb = appDb;
            Cache = cache;
            FileTransformer = fileTransformer;
        }
        
        public abstract Task Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext, CancellationToken cancellationToken) 
            where TActivity : IActivity;
        
        protected async Task HandleCore<TActivity>(CurrentDisplayQuestioner currentQuestioner, List<CardAction> additionalActions, 
            ITurnContext<TActivity> turnContext, CancellationToken cancellationToken) 
            where TActivity : IActivity
        {
            var child = currentQuestioner.Child;
            
            var actions = child
                .Select(it =>
                {
                    var marker = BotMessageMarker.Question(it.Id);
                    return CreateCardAction(it.Question, marker);
                })
                .ToList();
            
            var currentQuestionerActivity = await ToActivity(currentQuestioner);
            if (currentQuestionerActivity != null)
                await turnContext.SendActivityAsync(currentQuestionerActivity, cancellationToken);
            
            if (actions.Count <= 0)
                return;

            if(additionalActions != null)
                actions.AddRange(additionalActions);
            
            var heroCard = new HeroCard("Список вопросов", buttons: actions);
            var msg = MessageFactory.Attachment(Enumerable.Empty<Microsoft.Bot.Schema.Attachment>());
            msg.Attachments.Add(heroCard.ToAttachment());
            await turnContext.SendActivityAsync(msg, cancellationToken);
        }
        
        protected Task<List<Attachment>> SelectAttachment(Guid id, CancellationToken cancellationToken)
        {
            return AppDb.MessageAttachments
                .Include(e => e.File)
                .AsNoTracking()
                .Where(e => e.CrossEntityId == id)
                .Select(e => new Attachment(e.Id))
                .ToListAsync(cancellationToken: cancellationToken);
        }
        
        protected Task<List<Questioner>> AllChildFor(Guid? id, CancellationToken cancellationToken = default)
        {
            return Filter(e => e.ParentId == id)
                .Select(e => new Questioner(e.Id, e.Question, e.Answer))
                .ToListAsync(cancellationToken: cancellationToken);
        }

        protected IQueryable<BotQuestioner> Filter(Expression<Func<BotQuestioner, bool>> filter)
        {
            return AppDb.Questioner.AsNoTracking().Where(filter);
        }
        
        private async Task EnrichAttachment(HeroCard card, IList<Microsoft.Bot.Schema.Attachment> returnAttachments, 
            List<Attachment> attachments)
        {
            foreach (var attachment in attachments)
            {
                var mapInfo = await BuildMapInfo(attachment);
                if (mapInfo.IsOk)
                {
                    var val = mapInfo.Value;
                    if (val.IsImage)
                    {
                        card.Images.Add(new CardImage
                        {
                            Url = val.Url
                        });   
                    }
                    else
                    {
                        returnAttachments.Add(new Microsoft.Bot.Schema.Attachment
                        {
                            ContentType = val.ContentType,
                            Name = val.Name,
                            ContentUrl = val.Url
                        });
                    }
                }
                else
                {
                    Logger.LogError("CreateImageAttachment: {Fail}", mapInfo.Error);
                }
            }
        }
        
        protected static CardAction CreateCardAction(string title, IBotMessage marker)
        {
            return new(ActionTypes.PostBack, title, value: marker);
        }

        private async Task<IActivity> ToActivity(CurrentDisplayQuestioner questioner)
        {
            if (questioner.Attachments.Count == 0 && string.IsNullOrWhiteSpace(questioner.Answer))
                return null;
            
            var msg = MessageFactory.Attachment(Enumerable.Empty<Microsoft.Bot.Schema.Attachment>());

            var card = new HeroCard(images: new List<CardImage>());

            if (!string.IsNullOrWhiteSpace(questioner.Answer))
                card.Text = questioner.Answer;
                    
            if (questioner.Attachments.Count > 0)
                await EnrichAttachment(card, msg.Attachments, questioner.Attachments);

            msg.Attachments.Add(card.ToAttachment());
            
            return msg;
        }
        
        private async Task<Result<AttachmentMapInfo, string>> BuildMapInfo(Attachment mapAttachment)
        {
            var res = await Cache.GetOrCreateAsync(mapAttachment, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                    
                var attachment = await AppDb.MessageAttachments
                    .AsNoTracking()
                    .Include(e => e.File)
                    .Where(e => e.Id == mapAttachment.Id)
                    .Select(e => e.File)
                    .FirstOrDefaultAsync();

                var convertResult = FileTransformer.ToBase64(attachment);
                if (convertResult.IsFail)
                {
                    Result<AttachmentMapInfo, string> fail = Result.Fail(convertResult.FailMessage);
                    return Box.From(fail); 
                }
                    
                Result<AttachmentMapInfo, string> result = Result.Success(
                        new AttachmentMapInfo(MimeTypesMap.GetMimeType(attachment.Extension),
                            attachment.Name,
                            convertResult.Value,
                            MimeTypesMap.ImageExtensions.Contains(attachment.Extension))
                    );

                return Box.From(result);
            });

            return res.Value;
        }

        private record AttachmentMapInfo(string ContentType, string Name, string Url, bool IsImage);
        protected record Questioner(Guid Id, string Question, string Answer);
        
        protected record Attachment(Guid Id);

        protected record CurrentDisplayQuestioner(string Answer, List<Attachment> Attachments, List<Questioner> Child);
    }
}