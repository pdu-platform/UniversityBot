using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UniversityBot.EF;
using UniversityBot.Infrastructure.WordProcessing;
using UniversityBot.ServiceProvider.Configuration;

namespace UniversityBot.Test
{
    public static class TestServiceProviderExt
    {
        public static IServiceCollection ConfigureUniversityBotCoreForTest(this IServiceCollection self, WordLemmatizerFixture fixture)
        {
            var op = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
           
            var ctx = new AppDbContext(op);
            
            return self
                .ConfigureUniversityBotCore(ctx, new DummyContextFactory(), false)
                .Replace(new ServiceDescriptor(typeof(AppDbContext), ctx))
                .Replace(new ServiceDescriptor(typeof(WordLemmatizer), fixture.WordLemmatizer));
        }
    }
}