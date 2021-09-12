using System;
using Microsoft.Extensions.DependencyInjection;

namespace UniversityBot.Core.Command.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SlashCommandHandlerAttribute : CommandHandlerAttribute
    {
        public SlashCommandHandlerAttribute(string command, string userFriendlyName, ServiceLifetime lifetime = ServiceLifetime.Transient, bool showInAllCommandList = true) 
            : base(command != null ? "/" + command : null, userFriendlyName, lifetime, showInAllCommandList)
        {
        }
    }
}