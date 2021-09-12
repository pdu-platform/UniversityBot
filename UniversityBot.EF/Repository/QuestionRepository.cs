using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blowin.Optional;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Core.DAL;
using UniversityBot.Core.Util;

namespace UniversityBot.EF.Repository
{
    public sealed class QuestionRepository
    {
        private readonly AppDbContext _db;

        public QuestionRepository(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<BotQuestioner> Query() => _db.Questioner;
        
        public ValueTask<BotQuestioner> FindAsync(Guid id) => _db.Questioner.FindAsync(id);

        public async Task<List<Guid>> AllChildForQuestionAsync(Guid id)
        {
            var result = new List<Guid>();
            await AppendAllChildIdForQuestionAsync(id, result);
            return result;
        }

        public async ValueTask AddAsync(BotQuestioner questioner)
        {
            await _db.Questioner.AddAsync(questioner);
        }

        public void Update(BotQuestioner questioner)
        {
            _db.Questioner.Update(questioner);
        }
        
        /// <param name="id">Question Id</param>
        /// <returns>Return error msg</returns>
        public async Task<Optional<string>> DeleteWithoutChildAsync(Guid id)
        {
            var question = await _db.Questioner.FindAsync(id);
            if (question == null)
                return Optional.From("Не найден вопрос");

            var curQuestionParent = question.ParentId;

            var allChild = await Query().Where(e => e.ParentId == id).ToListAsync();
            
            foreach (var botQuestion in allChild)
                botQuestion.ParentId = curQuestionParent;

            _db.Questioner.UpdateRange(allChild);
            _db.Questioner.Remove(question);
            
            return Optional.None();
        }

        /// <param name="id">Question Id</param>
        /// <returns>Return error msg</returns>
        public async Task<Optional<string>> DeleteWithChildAsync(Guid id)
        {
            var question = await _db.Questioner.FindAsync(id);
            if (question == null)
                return Optional.From("Не найден вопрос");

            var allChildIds = await AllChildForQuestionAsync(id);
            var allChild = await Query().Where(e => allChildIds.Contains(e.Id)).ToListAsync();
            
            _db.Questioner.RemoveRange(allChild);
            _db.Questioner.Remove(question);

            return Optional.None();
        }

        private async Task AppendAllChildIdForQuestionAsync(Guid id, List<Guid> resultStore)
        {
            var firstLevel = await Query()
                .AsNoTracking()
                .Where(e => e.ParentId == id)
                .Select(e => e.Id)
                .ToListAsync();

            foreach (var firstLevelId in firstLevel)
            {
                resultStore.Add(firstLevelId);

                await AppendAllChildIdForQuestionAsync(firstLevelId, resultStore);
            }
        }
    }
}