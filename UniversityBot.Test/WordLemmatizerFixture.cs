using System;
using Microsoft.Extensions.Logging.Abstractions;
using UniversityBot.Infrastructure.WordProcessing;

namespace UniversityBot.Test
{
    public class WordLemmatizerFixture : IDisposable
    {
        public WordLemmatizer WordLemmatizer { get; }

        public WordLemmatizerFixture()
        {
            WordLemmatizer = new WordLemmatizer(new NullLogger<WordLemmatizer>());
        }

        public void Dispose()
        {
        }
    }
}