using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Core.DAL;
using UniversityBot.EF.Extension;

namespace UniversityBot.EF.Repository
{
    public sealed class CommandRepository
    {
        private readonly AppDbContext _db;

        public CommandRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<BotCommandPage>> SelectPage(int pageIndex, int pageSize,
            Expression<Func<BotCommand, bool>> filter = null, CancellationToken token = default)
        {
            return _db.Commands
                .Include(e => e.Questions)
                .SelectPageAsync(pageIndex, pageSize, e => Queryable.OrderBy(e, t => t.UserFriendlyName),
                    e => new BotCommandPage
                    {
                        Id = e.Id,
                        HandleNames = Enumerable.Select(e.Questions, e => new BotHandleName
                        {
                            Id = e.Id,
                            Question = e.Question
                        }).ToList(),
                        UserFriendlyName = e.UserFriendlyName,
                        Answer = e.Answer,
                        ShowInAllCommandList = e.ShowInAllCommandList
                    }, filter, token);
        }
        
        public async Task DeleteChildEntity(Guid commandId)
        {
            var removeSet = await _db.Commands
                .AsNoTracking()
                .Include(e => e.Questions)
                .Where(e => e.Id == commandId)
                .Select(e => new
                {
                    HandleNames = e.Questions.Select(hn => hn.Id).ToList()
                })
                .FirstOrDefaultAsync();

            if(removeSet == null)
                return;
            
            if (removeSet.HandleNames.Count > 0)
            {
                var removeHandleNamesInf = removeSet.HandleNames.Select(BotCommandQuestion.ForRemove);
                _db.RemoveByTrackInfo(_db.CommandHandleNames, removeHandleNamesInf);
            }
        }
        
        public void DeleteCommand(Guid commandId)
        {
            _db.RemoveByTrackInfo(_db.Commands, BotCommand.ForRemove(commandId));
        }
        
        public Task<BotCommand> Create(string userFriendlyName, string commandResult, IEnumerable<string> handleNames)
        {
            var command = new BotCommand(Guid.Empty, userFriendlyName, true, new List<BotCommandQuestion>(), commandResult);
            foreach (var botCommandHandleName in handleNames.Select(handleName => new BotCommandQuestion(Guid.Empty, handleName, command.Id, command)))
                command.Questions.Add(botCommandHandleName);

            return Create(command);
        }
        
        public async Task<BotCommand> Create(BotCommand command)
        {
            await _db.Commands.AddAsync(command);

            if (command.Questions is not {Count: > 0}) 
                return command;
            
            foreach (var botCommandQuestion in command.Questions)
                botCommandQuestion.BotCommand = command;
             
            await _db.CommandHandleNames.AddRangeAsync(command.Questions);

            return command;
        }

        public void Update(BotCommand command)
        {
            if ((command.Questions?.Count ?? 0) > 0)
                _db.UpdateByTrackInfo(_db.CommandHandleNames, command.Questions);
            
            _db.UpdateByTrackInfo(_db.Commands, new []{ command });
        }
        
        public sealed record BotHandleName
        {
            public Guid Id { get; init; }
            public string Question { get; init; }
        }

        public sealed record BotCommandPage
        {
            public Guid Id { get; init; }
            public List<BotHandleName> HandleNames { get; init; }
            public string UserFriendlyName { get; init; }
            public string Answer { get; init; }
            public bool ShowInAllCommandList { get; init; }
        }
    }
}