using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniversityBot.Core.Command.Attributes;
using UniversityBot.EF;
using UniversityBot.Infrastructure.Command.CommandHandlers;
using UniversityBot.Infrastructure.Extension;

namespace UniversityBot.Infrastructure.Command
{
    public sealed class CommandHandlerMetadataStore
    {
        private readonly CommandDescriptor[] _commandDescriptors;

        public IEnumerable<CommandDescriptor> Descriptors => _commandDescriptors;
        
        public IEnumerable<(Type Handler, ServiceLifetime Lifetime)> ServiceDescription =>
            _commandDescriptors.Select(e => (e.CommandType, e.Lifetime));
        
        public IEnumerable<(string[] Command, string UserFriendlyName)> Names =>
            _commandDescriptors
                .Where(e => e.ShowInAllCommandList)
                .Select(e => (Command: e.Commands, e.UserFriendlyName));
        
        public CommandHandlerMetadataStore(IReadOnlyList<CommandDescriptor> commandDescriptors)
        {
            _commandDescriptors = LoadDescriptors(commandDescriptors);
        }
        
        private static CommandDescriptor[] LoadDescriptors(IEnumerable<CommandDescriptor> commandDescriptors)
        {
            var types = GetCommandHandlers();

            ThrowIfExists(types, e => e.Item1 == null || e.Item1.Length == 0, invalidClasses => $"Классы [{invalidClasses}] не содержат атрибут с коммандой");
            ThrowIfExists(types, e => e.Item1.Length > 1, invalidClasses => $"Классы [{invalidClasses}] содержат больше 1 атрибута с описанием обработчика");

            var result = types
                .Select(e =>
                {
                    var atr = e.Attributes[0];
                    return new CommandDescriptor(atr.Commands, atr.UserFriendlyName, atr.Lifetime, atr.ShowInAllCommandList, e.Type);
                })
                .Concat(commandDescriptors)
                .ToArray();

            ThrowIfHasDuplicates(result);
            
            return result;
        }

        private static void ThrowIfHasDuplicates(CommandDescriptor[] result)
        {
            var failResult = result
                .SelectMany(e => e.Commands)
                .AllDuplicates()
                .JoinToString(", ", prefix: "Дублирующиеся комманды: ");

            if (!string.IsNullOrEmpty(failResult))
                throw new ArgumentException(failResult);
        }
        
        private static (CommandHandlerAttribute[] Attributes, Type Type)[] GetCommandHandlers()
        {
            var commandInterface = typeof(ICommandHandler);
            var ignoreAttributeType = typeof(IgnoreCommandHandlerAttribute);
            
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .AsParallel()
                .SelectMany(asm => asm.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && commandInterface.IsAssignableFrom(t) && t.GetCustomAttribute(ignoreAttributeType) == null)
                .Select(e => (Attributes: e.GetCustomAttributes<CommandHandlerAttribute>()?.ToArray(), Type: e))
                .ToArray();
        }
        
        private static void ThrowIfExists<T>(IEnumerable<(T Attributes, Type Type)> types, 
            Func<(T, Type Type), bool> filter, 
            Func<string, string> formatMsg)
        {
            var invalidTypes = types.Where(filter).Select(e => e.Item2.Name);
            var invalidClasses = string.Join(", ", invalidTypes);
            if(!string.IsNullOrEmpty(invalidClasses))
                throw new InvalidOperationException(formatMsg(invalidClasses));
        }

        public static CommandHandlerMetadataStore Create(AppDbContext appDb, bool includeFromDatabase)
        {
            var commands = new List<CommandDescriptor>();
            if (includeFromDatabase)
            {
                commands = appDb
                    .Commands
                    .Include(e => e.Questions)
                    .AsNoTracking()
                    .ToList()
                    .Select(e =>
                    {
                        return new CommandDescriptor(
                            e.Questions.Select(n => n.Question).ToArray(),
                            e.UserFriendlyName,
                            ServiceLifetime.Singleton,
                            e.ShowInAllCommandList,
                            typeof(DatabaseCommandHandler)
                        );
                    })
                    .ToList();
            }

            return new CommandHandlerMetadataStore(commands);
        }

        public sealed record CommandDescriptor(string[] Commands, string UserFriendlyName, ServiceLifetime Lifetime,
            bool ShowInAllCommandList, Type CommandType);
    }
}