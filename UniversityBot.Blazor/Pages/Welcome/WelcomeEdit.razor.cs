using System;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Core.DAL;
using UniversityBot.EF;

namespace UniversityBot.Blazor.Pages.Welcome
{
    public partial class WelcomeEdit
    {
        [Parameter] public Guid Id { get; set; }
        private UnitOfWork _unitOfWork;
        private BotWelcome _welcome;
        private Form<BotWelcome> _form;

        private bool IsNew => Id == Guid.Empty;

        protected override async Task OnInitializedAsync()
        {
            _unitOfWork = new UnitOfWork(Context);
            
            if (IsNew)
            {
                _welcome = new BotWelcome(Guid.Empty, string.Empty, 0);
            }
            else
            {
                _welcome = await Context.Welcome.AsNoTracking().FirstAsync(e => e.Id == Id);
            }
        }

        private async Task HandleValidSubmit(EditContext arg)
        {
            if (IsNew)
            {
                await using (_unitOfWork.AsyncSaveScope())
                    await _unitOfWork.WelcomeRepository.AddAsync(_welcome);

                _unitOfWork.Database.Entry(_welcome).State = EntityState.Detached;
            }
            else
            {
                await using (_unitOfWork.AsyncSaveScope())
                    _unitOfWork.WelcomeRepository.Update(_welcome);
                
                _unitOfWork.Database.ChangeTracker.Clear();
            }
            
            NavigationManager.NavigateTo("/welcome");
        }
    }
}