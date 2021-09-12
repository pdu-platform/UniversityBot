using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Blazor.Extension;
using UniversityBot.Core.DAL;

using static UniversityBot.Blazor.Extension.ConverterFactory;

namespace UniversityBot.Blazor.Pages
{
    public partial class Settings
    {
        private Model _settings;
        private double InputNumber { get; set; }
        private string _error;
        
        protected override async Task OnInitializedAsync()
        {
            var loadSettings = await UnitOfWork.Database.Settings.AsNoTracking().FirstAsync();
            _settings = loadSettings.Map(To<Model>());
            
            InputNumber = _settings.MinKeywordMatchCount;
        }

        private async Task Save()
        {
            if (!BotSettings.ValidateMinKeywordMatchCount((uint) InputNumber, out var error))
            {
                _error = error;
                StateHasChanged();
                return;
            }
            
            var settings = new BotSettings(_settings.Id, _settings.MinKeywordMatchCount)
            {
                SplitWelcomeMessage = _settings.SplitWelcomeMessage,
                UseFuzzySearch = _settings.UseFuzzySearch,
                NotFoundAnswerMessage = _settings.NotFoundAnswerMessage,
                OnlyAnswerWithMaxMatch = _settings.OnlyAnswerWithMaxMatch,
                DontDisplayChildAndParentQuestionInAnswer = _settings.DontDisplayChildAndParentQuestionInAnswer,
            };
            
            UnitOfWork.Database.Settings.Attach(settings);
            try
            {
                UnitOfWork.Database.Settings.Update(settings);
                await UnitOfWork.SaveChangesAsync();
            }
            finally
            {
                UnitOfWork.Database.Entry(settings).State = EntityState.Detached;   
            }
            
            NavigationManager.NavigateTo("/");
        }
        
        private sealed class Model
        {
            public Guid Id { get; set; }
            public uint MinKeywordMatchCount { get; set; }
            public bool OnlyAnswerWithMaxMatch { get; set; }
            public bool DontDisplayChildAndParentQuestionInAnswer { get; set; }
            public bool SplitWelcomeMessage { get; set; }
            public string NotFoundAnswerMessage { get; set; }
            public bool UseFuzzySearch { get; set; }
        }
    }
}