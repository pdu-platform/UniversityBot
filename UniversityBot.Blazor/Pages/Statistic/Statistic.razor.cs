using UniversityBot.EF;
using UniversityBot.Infrastructure;

namespace UniversityBot.Blazor.Pages.Statistic
{
    public partial class Statistic
    {
        private UnitOfWork _unitOfWork;
        private StatisticTableSource _statisticTableSource;
        
        protected override void OnInitialized()
        {
            _unitOfWork = new UnitOfWork(Context);
            _statisticTableSource = new StatisticTableSource(StateHasChanged, _unitOfWork);
        }
    }
}