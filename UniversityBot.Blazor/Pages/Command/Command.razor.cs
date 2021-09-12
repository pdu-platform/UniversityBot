using UniversityBot.EF;
using UniversityBot.Infrastructure;

namespace UniversityBot.Blazor.Pages.Command
{
    public partial class Command
    {
        private CommandTableSource _commandTableSource;

        protected override void OnInitialized()
        {
            var unitOfWork = new UnitOfWork(Context);
            _commandTableSource = new CommandTableSource(unitOfWork, StringBuilderPool, StateHasChanged, NavigationManager);
        }
    }
}