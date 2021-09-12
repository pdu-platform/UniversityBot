using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Core.DAL;
using UniversityBot.EF;
using UniversityBot.EF.Extension;
using UniversityBot.Infrastructure;
using UniversityBot.Infrastructure.Extension;
using UniversityBot.Infrastructure.Validator;

namespace UniversityBot.Blazor.Pages.Command
{
    public partial class CommandEdit
    {
        [Parameter] public Guid Id { get; set; }

        private BotCommand _command;
        private Form<BotCommand> _form;
        private UnitOfWork _unitOfWork;
        private BotCommandValidator _botCommandValidator;
        private string _error;
        private List<Guid> _dbOnStartQuestions;

        private string CreateHandleName { get; set; }
        
        private bool IsNew => Id == Guid.Empty;

        protected override async Task OnInitializedAsync()
        {
            _botCommandValidator = new BotCommandValidator(Context, CommandHandlerStore);
            _unitOfWork = new UnitOfWork(Context);

            _dbOnStartQuestions = new List<Guid>();
            
            if (IsNew)
            {
                _command = new BotCommand(Guid.Empty, string.Empty, true, new List<BotCommandQuestion>(), 
                    string.Empty);
            }
            else
            {
                _command = await Context.Commands
                    .Include(e => e.Questions)
                    .AsNoTracking()
                    .FirstAsync(e => e.Id == Id);

                if (_command.Questions is {Count: > 0})
                {
                    foreach (var botCommandQuestion in _command.Questions)
                        _dbOnStartQuestions.Add(botCommandQuestion.Id);
                }
            }
        }

        private async Task HandleValidSubmit(EditContext obj)
        {
            _error = null;
            StateHasChanged();

            var result = await _botCommandValidator.ValidateAsync(_command);
            _error = result.IsValid
                ? string.Empty
                : result.Errors.JoinToString(Environment.NewLine, failure => failure.ErrorMessage);
            
            if (!string.IsNullOrEmpty(_error))
                return;

            if (IsNew)
            {
                await Create(obj);
            }
            else
            {
                await Update(obj);
            }
            
            NavigationManager.NavigateTo("/command");
        }
        
        private async Task Create(EditContext obj)
        {
            await using (_unitOfWork.AsyncSaveScope())
                await _unitOfWork.CommandRepository.Create(_command);

            _unitOfWork.Database.Entry(_command).State = EntityState.Detached;
        }

        private void AddHandleName()
        {
            if (string.IsNullOrEmpty(CreateHandleName))
                return;

            if (_command.Questions.Any(e => e.Question == CreateHandleName))
                return;

            _command.Questions.Add(new BotCommandQuestion(Guid.Empty, CreateHandleName, Id, null));
            StateHasChanged();
        }
        
        private void DeleteHandleName(BotCommandQuestion question)
        {
            if (question == null)
                return;

            var idx = _command.Questions.FindIndex(e => ReferenceEquals(e, question));
            if(idx >= 0)
                _command.Questions.RemoveAt(idx);
        }
        
        private async Task Update(EditContext obj)
        {
            _error = null;
            StateHasChanged();

            if (!_form.Validate())
                return;

            await using (_unitOfWork.AsyncSaveScope())
            {
                var removeItems = new HashSet<Guid>(_dbOnStartQuestions);
                removeItems.ExceptWith(_command.Questions.Where(e => e.Id != Guid.Empty).Select(e => e.Id));

                if (removeItems.Count > 0)
                {
                    var removeInfo = removeItems.Select(BotCommandQuestion.ForRemove);
                    _unitOfWork.Database.RemoveByTrackInfo(_unitOfWork.Database.CommandHandleNames, removeInfo);
                }

                var insertQuestions = _command.Questions.Where(botCommandHandleName => botCommandHandleName.Id == Guid.Empty);
                await _unitOfWork.Database.CommandHandleNames.AddRangeAsync(insertQuestions);
            }

            await using (_unitOfWork.AsyncSaveScope())
                _unitOfWork.CommandRepository.Update(_command);
            
            _unitOfWork.Database.ChangeTracker.Clear();
        }
    }
}