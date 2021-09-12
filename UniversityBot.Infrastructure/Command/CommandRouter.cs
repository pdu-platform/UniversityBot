using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blowin.Optional;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using UniversityBot.Core.BotMessage;
using UniversityBot.Core.Enums;
using UniversityBot.Core.Statistic;
using UniversityBot.Infrastructure.Command.CommandHandlers;
using UniversityBot.Infrastructure.Command.CommandHandlers.Questioner;
using UniversityBot.Infrastructure.WordProcessing;

namespace UniversityBot.Infrastructure.Command
{
    public sealed class CommandRouter
    {
        private readonly IServiceProvider _service;
        private readonly Dictionary<string, (Type Type, ServiceLifetime Lifetime)> _commandRouter;
        private readonly KeywordAnalyzer _keywordAnalyzer;

        public CommandRouter(IServiceProvider service, CommandHandlerMetadataStore metadataStore,
            KeywordAnalyzer keywordAnalyzer)
        {
            _keywordAnalyzer = keywordAnalyzer;
            _service = service;

            _commandRouter = metadataStore
                .Descriptors
                .SelectMany(e => e.Commands.Select(c => (Command: c, Type: e.CommandType, e.Lifetime)))
                .ToDictionary(e => e.Command, e => (e.Type, e.Lifetime));
                        
            _commandRouter.TrimExcess();
        }

        public static Optional<CommandRequest> CreateHandleRequest(string command, IBotMessage message)
        {
            var cmd = command;
            if (string.IsNullOrEmpty(cmd) && !string.IsNullOrEmpty(message?.HandlerName))
                cmd = message.HandlerName;

            if (string.IsNullOrEmpty(cmd))
                return Optional.None();

            return new CommandRequest(cmd, message).AsOptional();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TActivity"></typeparam>
        /// <returns>True - ответ найден</returns>
        public async Task<bool> Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext,
            CancellationToken cancellationToken)
            where TActivity : IActivity
        {
            if (_commandRouter.TryGetValue(request.Command, out var info))
            {
                await Handle(request, turnContext, info.Type, info.Lifetime, cancellationToken);
                return true;
            }

            if (request.Message is { Type: BotMessageType.UserSearch })
            {
                var questionerList = _keywordAnalyzer.Analyze(request.Command).ToArray();
                if (questionerList.Length == 0)
                {
                    await HandleNotFoundWord(request);
                    return false;
                }
                
                var rk = BotMessageMarker.ReactionKeyword(questionerList);
                var newRequest = new CommandRequest(request.Command, rk);
                await Handle(newRequest, turnContext, typeof(QuestionerReactionKeywordHandler), ServiceLifetime.Scoped, cancellationToken);
                return true;
            }

            return false;
        }
        
        private Task Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext, Type type,
            ServiceLifetime lifetime, CancellationToken cancellationToken)
            where TActivity : IActivity
        {
            if (lifetime == ServiceLifetime.Scoped)
            {
                using var scope = _service.CreateScope();
                var handler = (ICommandHandler) scope.ServiceProvider.GetRequiredService(type);
                return handler.Handle(request, turnContext, cancellationToken);
            }
            else
            {
                var handler = (ICommandHandler) _service.GetRequiredService(type);
                return handler.Handle(request, turnContext, cancellationToken);
            }
        }
        
        private Task HandleNotFoundWord(in CommandRequest request)
        {
            using var scope = _service.CreateScope();
                    
            var store = scope.ServiceProvider.GetRequiredService<INotFoundWordStore>();
                    
            var (isOk, value, _) = NotFoundStoreEntity.Create(request.Command);
            return isOk ? store.Save(value) : Task.CompletedTask;
        }
    }
}