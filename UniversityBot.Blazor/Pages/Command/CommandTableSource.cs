using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using UniversityBot.Blazor.Components.Table;
using UniversityBot.EF;
using UniversityBot.Infrastructure;
using UniversityBot.Infrastructure.Extension;

namespace UniversityBot.Blazor.Pages.Command
{
    public sealed class CommandTableSource : ITableComponentSource<CommandTableSource.TableModel>
    {
        public int PageSize => 15;

        public bool EnableCreateCommand => true;
        public bool EnableEditCommand => true;
        public bool EnableDeleteCommand => true;
        
        private readonly UnitOfWork _unitOfWork;
        private readonly ObjectPool<StringBuilder> _stringBuilderPool;
        private readonly Action _stateChanged;
        private readonly NavigationManager _navigationManager;
        
        public CommandTableSource(UnitOfWork unitOfWork, ObjectPool<StringBuilder> stringBuilderPool, Action stateChanged, NavigationManager navigationManager)
        {
            _unitOfWork = unitOfWork;
            _stringBuilderPool = stringBuilderPool;
            _stateChanged = stateChanged;
            _navigationManager = navigationManager;
        }

        public int GetTotal() => _unitOfWork.Database.Commands.AsNoTracking().Count();

        public async Task<List<TableModel>> GetTableModel(int pageIndex, int pageSize)
        {
            var list = await _unitOfWork.CommandRepository.SelectPage(pageIndex, pageSize);

            using var stringBuilderItem = _stringBuilderPool.GetScoped();
            var stringBuilder = stringBuilderItem.Item;
            return list
                .Select(e =>
                {
                    var handleNames = e.HandleNames.Select(e => (e.Id, e.Question)).ToList();
                    var name = JoinHandleNames(stringBuilder.Clear(), handleNames.Select(t => t.Question));
                    return new TableModel(e.Id, e.UserFriendlyName, name, handleNames, e.Answer, e.ShowInAllCommandList);
                })
                .ToList();
        }

        public async Task Delete(List<TableModel> modelList, TableModel model)
        {
            var id = model.Id;
            if (modelList.Count == 0 || id == Guid.Empty)
                return;

            await using (_unitOfWork.AsyncSaveScope())
                await _unitOfWork.CommandRepository.DeleteChildEntity(id);

            await using (_unitOfWork.AsyncSaveScope())
                _unitOfWork.CommandRepository.DeleteCommand(id);

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

            _navigationManager.NavigateTo($"/command/edit/{context.Id}");
            return Task.CompletedTask;
        }

        public void Create()
        {
            _navigationManager.NavigateTo($"/command/edit/{Guid.Empty}");
        }

        private static string JoinHandleNames(StringBuilder builder, IEnumerable<string> names)
            => builder.JointToString(names, separator: ", ").ToString();
        
        public sealed class TableModel
        {
            public Guid Id { get; set; }

            [DisplayName("Название")] public string UserFriendlyName { get; set; }

            [DisplayName("Команды")] public string JoinHandleNames { get; set; }

            [DisplayName("Ответ")] public string Answer { get; set; }
            
            [DisplayName("Отображать в списке команд")] public bool ShowInAllCommandList { get; set; }
            
            public List<(Guid Id, string Name)> HandleNames { get; set; }

            public TableModel(Guid id, string command, string joinHandleNames, List<(Guid Id, string Name)> handleNames,
                string answer, bool showInAllCommandList)
            {
                UserFriendlyName = command;
                JoinHandleNames = joinHandleNames;
                Answer = answer;
                ShowInAllCommandList = showInAllCommandList;
                HandleNames = handleNames ?? new List<(Guid, string)>();
                Id = id;
            }
        }
    }
}