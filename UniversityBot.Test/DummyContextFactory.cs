using System;
using Microsoft.EntityFrameworkCore;
using UniversityBot.EF;

namespace UniversityBot.Test
{
    public class DummyContextFactory : IContextFactory
    {
        public AppDbContext Create(DbContextOptionsBuilder optionBuilderConfigurator)
        {
            throw new InvalidOperationException();
        }
    }
}