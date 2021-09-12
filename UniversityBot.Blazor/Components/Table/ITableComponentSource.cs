using System.Collections.Generic;
using System.Threading.Tasks;

namespace UniversityBot.Blazor.Components.Table
{
    public interface ITableComponentSource<TModel>
    {
        bool EnableEditCommand { get; }
        bool EnableDeleteCommand { get; }
        bool EnableCreateCommand { get; }
        
        int PageSize { get; }
        
        int GetTotal();
        
        Task<List<TModel>> GetTableModel(int pageIndex, int pageSize);

        Task Delete(List<TModel> modelList, TModel model);

        Task EditCommand(List<TModel> model, TModel context);

        void Create();
    }
}