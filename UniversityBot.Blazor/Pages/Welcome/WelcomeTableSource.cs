using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Blazor.Components.Table;
using UniversityBot.EF;
using UniversityBot.Infrastructure;

namespace UniversityBot.Blazor.Pages.Welcome
{
    public class WelcomeTableSource : ITableComponentSource<WelcomeTableSource.TableModel>
    {
        public int PageSize => 15;
        
        public bool EnableEditCommand => true;
        public bool EnableDeleteCommand => true;
        public bool EnableCreateCommand => true;
        
        private readonly Action _stateChanged;
        private readonly UnitOfWork _unitOfWork;
        private readonly NavigationManager _navigationManager;
        
        public WelcomeTableSource(UnitOfWork unitOfWork, NavigationManager navigationManager, Action stateChanged)
        {
            _unitOfWork = unitOfWork;
            _navigationManager = navigationManager;
            _stateChanged = stateChanged;
        }

        public int GetTotal() => _unitOfWork.Database.Welcome.AsNoTracking().Count();

        public Task<List<TableModel>> GetTableModel(int pageIndex, int pageSize)
        {
            return _unitOfWork.WelcomeRepository.SelectPageAsync(pageIndex, pageSize, 
                welcome => new TableModel(welcome.Id, welcome.Text, welcome.Order));
        }

        public async Task Delete(List<TableModel> modelList, TableModel model)
        {
            var id = model.Id;
            if (modelList.Count == 0 || id == Guid.Empty)
                return;
            
            await using(_unitOfWork.AsyncSaveScope())
                _unitOfWork.WelcomeRepository.Delete(id);
         
            var idx = modelList.FindIndex(e => e.Id == id);
            if (idx >= 0)
            {
                modelList.RemoveAt(idx);
                _stateChanged();
            }
        }

        public Task EditCommand(List<TableModel> model, TableModel context)
        {
            if (model.Count == 0 || context == null)
                return Task.CompletedTask;

            _navigationManager.NavigateTo($"/welcome/edit/{context.Id}");
            return Task.CompletedTask;
        }

        public void Create() => _navigationManager.NavigateTo($"/welcome/edit/{Guid.Empty}");
        
        public sealed class TableModel
        {
            public Guid Id { get; set; }
            
            [DisplayName("Сообщение")] public string Text { get; set; }
            
            [DisplayName("Порядок")] 
            [Range(0, int.MaxValue, ErrorMessage = "Неверный порядковый номер")]
            public int Order { get; set; }

            public TableModel(Guid id, string text, int order)
            {
                Id = id;
                Text = text;
                Order = order;
            }
        }
    }
}