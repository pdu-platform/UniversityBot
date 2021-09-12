using Microsoft.Extensions.DependencyInjection;
using UniversityBot.Infrastructure.Command;

namespace UniversityBot.Infrastructure.Extension
{
    public static class ServiceCollectionCommandRouterExt
    {
        public static IServiceCollection AddCommandHandlers(this IServiceCollection self, CommandHandlerMetadataStore metadataStore)
        {
            foreach (var (commandType, lifeTime) in metadataStore.ServiceDescription)
            {
                var descriptor = new ServiceDescriptor(commandType, commandType, lifeTime);
                self.Add(descriptor);
            }
            
            return self;
        }
    }
}