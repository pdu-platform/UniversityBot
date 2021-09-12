using UniversityBot.EF;
using UniversityBot.Infrastructure;

namespace UniversityBot.Blazor.Pages.Welcome
{
    public partial class Welcome
    {
        private WelcomeTableSource _commandTableSource;
        
        protected override void OnInitialized()
        {
            var uow = new UnitOfWork(Context);
            _commandTableSource = new WelcomeTableSource(uow, NavigationManager, StateHasChanged);
        }
    }
}