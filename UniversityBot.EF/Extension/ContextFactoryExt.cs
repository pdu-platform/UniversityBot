using System;
using Microsoft.EntityFrameworkCore;

namespace UniversityBot.EF.Extension
{
    public static class ContextFactoryExt
    {
        public static AppDbContext Create(this IContextFactory self, Action<DbContextOptionsBuilder> optionConfigurator)
        {
            var option = new DbContextOptionsBuilder();
            optionConfigurator?.Invoke(option);
            return self.Create(option);
        }
    }
}