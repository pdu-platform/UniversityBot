using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using UniversityBot.Core.BotMessage;
using UniversityBot.Infrastructure.Command;
using UniversityBot.Infrastructure.Extension;
using Constants = UniversityBot.Core.Constants;

namespace UniversityBot.ASP.Bots
{
    public class QuizBot : ActivityHandler
    {
        private readonly ILogger<QuizBot> _logger;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly CommandRouter _commandRouter;

        public QuizBot(ILogger<QuizBot> logger, ConversationState conversationState, UserState userState,
            CommandRouter commandRouter)
        {
            _logger = logger;
            _conversationState = conversationState;
            _userState = userState;
            _commandRouter = commandRouter;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new())
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text;
            var botMessage = turnContext.Activity.ToBotMessage();

            var request = CommandRouter.CreateHandleRequest(text, botMessage ?? UserSearchMarker.Instance);

            if (request.IsNone)
            {
                await _commandRouter.Handle(new CommandRequest(Constants.Command.NotFoundAnswer), turnContext, cancellationToken);
                return;
            }

            var handle = await _commandRouter.Handle(request.Value, turnContext, cancellationToken);
            if (!handle)
                await _commandRouter.Handle(new CommandRequest(Constants.Command.NotFoundAnswer), turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var _ in membersAdded.Where(member => member.Id != turnContext.Activity.Recipient.Id))
            {
                await _commandRouter.Handle(new CommandRequest(Constants.Command.Hello), turnContext, cancellationToken);
                await _commandRouter.Handle(new CommandRequest(Constants.Command.Question), turnContext, cancellationToken);
            }
        }
    }
}