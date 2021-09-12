using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Core.DAL;
using UniversityBot.Core.Util;
using UniversityBot.EF;
using UniversityBot.Infrastructure.Command;

namespace UniversityBot.Infrastructure.Validator
{
    public sealed class HandleNameValidator : AbstractValidator<List<HandleNameValidator.Param>>
    {
        private AppDbContext _context;
        private CommandHandlerMetadataStore _commandHandlerStore;

        public HandleNameValidator(AppDbContext context, CommandHandlerMetadataStore commandHandlerStore)
        {
            _context = context;
            _commandHandlerStore = commandHandlerStore;
            
            RuleFor(e => e)
                .MustAsync(HandleNameBeUniq)
                .WithMessage("Такая команда для срабатывания уже существует");
        }
        
        private async Task<bool> HandleNameBeUniq(List<Param> handleName, CancellationToken token)
        {
            if (handleName.Count == 0)
                return true;
            
            if (_commandHandlerStore.Descriptors.AsParallel()
                .SelectMany(e => e.Commands)
                .Any(command => handleName.Any(e => e.Question == command)))
            {
                return false; 
            }

            var handleNameStr = handleName.Select(e => e.Question).ToList(handleName.Count);

            var invalidEntry = await _context.CommandHandleNames
                .AsNoTracking()
                .Where(e => handleNameStr.Contains(e.Question))
                .Select(e => new {e.Id, e.Question})
                .ToListAsync(cancellationToken: token);

            if (invalidEntry.Count == 0)
                return true;
            
            return !invalidEntry.Any(en => handleName.Any(hn => hn.Question == en.Question && en.Id != hn.Id));
        }
        
        public sealed class Param
        {
            public Guid Id { get; }
            public string Question { get; }

            public Param(Guid id, string question)
            {
                Id = id;
                Question = question;
            }
        }
    }

    public sealed class UserFriendlyNameValidator : AbstractValidator<UserFriendlyNameValidator.Param>
    {
        private AppDbContext _context;
        private CommandHandlerMetadataStore _commandHandlerStore;

        public UserFriendlyNameValidator(AppDbContext context, CommandHandlerMetadataStore commandHandlerStore)
        {
            _context = context;
            _commandHandlerStore = commandHandlerStore;
            
            RuleFor(e => e).MustAsync(CommandNameBeUniq).WithMessage("Такое название уже существует");
        }
        
        private async Task<bool> CommandNameBeUniq(Param param, CancellationToken token)
        {
            if (_commandHandlerStore.Descriptors.AsParallel().Any(e => e.UserFriendlyName == param.UserFriendlyName))
                return false;

            var isInvalid = await _context.Commands
                .AsNoTracking()
                .AnyAsync(e => e.UserFriendlyName == param.UserFriendlyName && e.Id != param.Id, cancellationToken: token);
            return !isInvalid;
        }
        
        public sealed class Param
        {
            public Guid Id { get; }
            public string UserFriendlyName { get; }

            public Param(Guid id, string userFriendlyName)
            {
                Id = id;
                UserFriendlyName = userFriendlyName;
            }
        }
    }
    
    public sealed class BotCommandValidator : AbstractValidator<BotCommand>
    {
        public BotCommandValidator(AppDbContext context, CommandHandlerMetadataStore commandHandlerStore)
        {
            RuleFor(e => e.Questions.Select(e => new HandleNameValidator.Param(e.Id, e.Question)).ToList())
                .SetValidator(new HandleNameValidator(context, commandHandlerStore));
            
            RuleFor(e => new UserFriendlyNameValidator.Param(e.Id, e.UserFriendlyName)).SetValidator(new UserFriendlyNameValidator(context, commandHandlerStore));
        }
    }
}