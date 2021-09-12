using System;
using Dawn;
using Microsoft.Extensions.DependencyInjection;

namespace UniversityBot.Core.Command.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandHandlerAttribute : Attribute
    {
        public string[] Commands { get; }
        public string UserFriendlyName { get; }
        public ServiceLifetime Lifetime { get; }
        public bool ShowInAllCommandList { get; }

        public CommandHandlerAttribute(string commands, string userFriendlyName, ServiceLifetime lifetime = ServiceLifetime.Transient, bool showInAllCommandList = true)
         : this(commands != null ? new []{ commands } : null, userFriendlyName, lifetime, showInAllCommandList)
        {
            
        }
        
        public CommandHandlerAttribute(string[] commands, string userFriendlyName, ServiceLifetime lifetime = ServiceLifetime.Transient, bool showInAllCommandList = true)
        {
            ShowInAllCommandList = showInAllCommandList;
            Lifetime = lifetime;
            
            Commands = Guard.Argument(commands, nameof(commands)).NotNull().MinCount(1);
            Commands = Commands;
            
            UserFriendlyName = userFriendlyName ?? Commands[0];
        }
    }
}