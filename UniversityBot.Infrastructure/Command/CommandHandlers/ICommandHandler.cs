using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace UniversityBot.Infrastructure.Command.CommandHandlers
{
    public interface ICommandHandler
    {
        Task Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext, CancellationToken cancellationToken)
            where TActivity : IActivity;
    }
}