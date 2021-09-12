using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using UniversityBot.Core.DAL;

namespace UniversityBot.Blazor.Components.Table
{
    public partial class TableComponent<TModel>
    {
        private int _total;
        private ITable _table;
        private int _pageIndex = 1;
        private int _pageSize = 10;
        
        [Parameter]
        public ITableComponentSource<TModel> Source { get; set; }
        
        [Parameter]
        public RenderFragment<TModel> Columns { get; set; }
        
        private List<TModel> Data { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _pageSize = Source.PageSize;
            _total = Source.GetTotal();
            Data = await Source.GetTableModel(_pageIndex, _pageSize);
        }

        private Task DeleteCommand(TModel model) => Source.Delete(Data, model);

        private Task EditCommand(TModel context) => Source.EditCommand(Data, context);

        private async Task ChangePageIndex(PaginationEventArgs arg)
        {
            Data = await Source.GetTableModel(arg.PageIndex, arg.PageSize);
        }

        private void Create() => Source.Create();
    }
}