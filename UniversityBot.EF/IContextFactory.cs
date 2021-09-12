using Microsoft.EntityFrameworkCore;

namespace UniversityBot.EF
{
    public interface IContextFactory
    {
        AppDbContext Create(DbContextOptionsBuilder optionBuilderConfigurator);
    }
}