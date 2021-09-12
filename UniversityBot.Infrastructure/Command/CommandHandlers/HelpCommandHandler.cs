using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using UniversityBot.Core.Command.Attributes;

namespace UniversityBot.Infrastructure.Command.CommandHandlers
{
    [CommandHandler(new []{ "help", "/help"}, "Помощь", ServiceLifetime.Singleton, false)]
    public sealed class HelpCommandHandler : ICommandHandler
    {
        private readonly (string Command, string UserFriendlyName)[] _names;
        
        public HelpCommandHandler(CommandHandlerMetadataStore commandHandlerMetadataStore)
        {
            _names = commandHandlerMetadataStore
                .Names
                .Select(e => (e.Command[0], e.UserFriendlyName))
                .ToArray();
        }
        
        public Task Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext, CancellationToken cancellationToken) where TActivity : IActivity
        {
            var msg = MessageFactory.Attachment(Enumerable.Empty<Attachment>());

            var card = new HeroCard
            {
                Text = "Команды",
                Buttons = _names
                    .Select(e => new CardAction(ActionTypes.PostBack, e.UserFriendlyName, value: e.Command))
                    .ToList(),
            };
            
            msg.Attachments.Add(card.ToAttachment());
            
            return turnContext.SendActivityAsync(msg, cancellationToken);
        }
    }
}