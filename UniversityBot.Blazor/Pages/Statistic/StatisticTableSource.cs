using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Blazor.Components.Table;
using UniversityBot.EF;
using UniversityBot.Infrastructure;

namespace UniversityBot.Blazor.Pages.Statistic
{
    public class StatisticTableSource : ITableComponentSource<StatisticTableSource.Model>
    {
        public int PageSize => 20;
     
        public bool EnableEditCommand => false;
        
        public bool EnableDeleteCommand => false;
        public bool EnableCreateCommand => false;
        
        private readonly Action _stateChanged;
        private readonly UnitOfWork _unitOfWork;

        public StatisticTableSource(Action stateChanged, UnitOfWork unitOfWork)
        {
            _stateChanged = stateChanged;
            _unitOfWork = unitOfWork;
        }

        public int GetTotal() => _unitOfWork.Database.NotFoundWords.AsNoTracking().Count();

        public async Task<List<Model>> GetTableModel(int pageIndex, int pageSize)
        {
            var res = await _unitOfWork.NotFoundWordRepository.SelectPageAsync(pageIndex, pageSize,
                word => new Model(word.Question, word.CreateTime));

            foreach (var model in res)
                model.CreateDate = model.CreateDate.ToLocalTime();

            return res;
        }

        public Task Delete(List<Model> modelList, Model model) => throw new Exception();

        public Task EditCommand(List<Model> model, Model context) => throw new Exception();

        public void Create() => throw new Exception();

        public sealed record Model(string Question, DateTime CreateDate)
        {
            [DisplayName("Вопрос")] 
            public string Question { get; set; } = Question;

            [DisplayName("Дата создания")] 
            public DateTime CreateDate { get; set; } = CreateDate;
        }
    }
}