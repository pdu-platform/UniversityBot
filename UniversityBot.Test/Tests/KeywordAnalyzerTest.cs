using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using UniversityBot.Core.DAL;
using UniversityBot.EF;
using UniversityBot.Infrastructure.WordProcessing;
using Xunit;

namespace UniversityBot.Test.Tests
{
    public class KeywordAnalyzerTest : IClassFixture<WordLemmatizerFixture>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _appDbContext;

        public KeywordAnalyzerTest(WordLemmatizerFixture wordLemmatizerFixture)
        {
            _serviceProvider = new ServiceCollection()
                .ConfigureUniversityBotCoreForTest(wordLemmatizerFixture)
                .BuildServiceProvider();
            
            _appDbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        }

        [Theory]
        [InlineData("чем заняться в свободное время", "Какие есть занятия?")]
        [InlineData("чем занться в свободное время", "Какие есть занятия?")]
        [InlineData("заняте спортом в сободне врем", "Какие есть занятия?")]
        [InlineData("занятие спортом в свободное время", "Какие есть занятия?")]
        public void Analyze_WithChild_And_Same_Keywords_OnlyWithMaxMatch_MinMatchCount1_UseFuzzySearch(string searchInput, string resultQuestion)
        {
            var settings = new BotSettings(Guid.Empty, 1)
            {
                OnlyAnswerWithMaxMatch = true,
                DontDisplayChildAndParentQuestionInAnswer = true,
                UseFuzzySearch = true
            };
            var analyzer = CreateKeywordAnalyzer(_serviceProvider, settings);
            
            var keywordFactory = _serviceProvider.GetRequiredService<KeywordFactory>();
            
            var rootQuestioner = CreateBotQuestioner("Чем заняться в свободное время", null, new[]
            {
                keywordFactory.Create("спорт"),
                keywordFactory.Create("занятие"),
                keywordFactory.Create("заняться"),
                keywordFactory.Create("скучно"),
                keywordFactory.Create("время"),
                keywordFactory.Create("свободное"),
                keywordFactory.Create("скука"),
            });

            _appDbContext.Questioner.Add(rootQuestioner);
            _appDbContext.SaveChanges();
            
            var childQuestioner = CreateBotQuestioner("Какие есть занятия?", rootQuestioner, new []
            {
                keywordFactory.Create("скучно"),
                keywordFactory.Create("время"),
                keywordFactory.Create("свободное"),
                keywordFactory.Create("спорт"),
                keywordFactory.Create("спортом"),
                keywordFactory.Create("занятие"),
                keywordFactory.Create("заняться"),
                keywordFactory.Create("скука"),
            });
            
            _appDbContext.Questioner.Add(childQuestioner);
            _appDbContext.SaveChanges();

            var questions = new[] {rootQuestioner, childQuestioner};
            
            var result = analyzer.Analyze(searchInput).ToList();

            Assert.Single(result);

            var expectResult = questions.Single(e => e.Question == resultQuestion);
            Assert.Equal(expectResult.Id, result[0]);
        }
        
        [Fact]
        public void Analyze_WithoutChild_OnlyWithMaxMatch_MinMatchCount2()
        {
            var settings = new BotSettings(Guid.Empty, 2)
            {
                OnlyAnswerWithMaxMatch = true,
                DontDisplayChildAndParentQuestionInAnswer = true
            };
            var analyzer = CreateKeywordAnalyzer(_serviceProvider, settings);
            
            var (_, childQuestioner, _) = CreateQuestioners(_appDbContext, _serviceProvider);
            
            var result = analyzer.Analyze("Как попасть на полоцкий вокзал").ToList();

            Assert.Single(result);
            Assert.Equal(childQuestioner.Id, result[0]);
        }
        
        [Fact]
        public void Analyze_WithChild_OnlyWithMaxMatch_MinMatchCount2()
        {
            var settings = new BotSettings(Guid.Empty, 2)
            {
                OnlyAnswerWithMaxMatch = true
            };
            var analyzer = CreateKeywordAnalyzer(_serviceProvider, settings);

            var (rootQuestioner, childQuestioner, _) = CreateQuestioners(_appDbContext, _serviceProvider);
            
            var result = analyzer.Analyze("Как попасть на полоцкий вокзал").ToList();

            Assert.Equal(2, result.Count);
            Assert.True(new []{childQuestioner.Id, rootQuestioner.Id}.OrderBy(e => e).SequenceEqual(result.OrderBy(e => e)));
        }

        [Fact]
        public void Analyze_WithChild_And_MinKeywordMatchCount1_And_NotOnlyWithMaxMatch()
        {
            var settings = new BotSettings(Guid.Empty, 1);
            var analyzer = CreateKeywordAnalyzer(_serviceProvider, settings);

            var (rootQuestioner, childQuestioner, withoutChildQuestioner) = CreateQuestioners(_appDbContext, _serviceProvider);
            
            var result = analyzer.Analyze("Как попасть на полоцкий вокзал").ToList();

            Assert.Equal(3, result.Count);
            Assert.True(new []{rootQuestioner.Id, childQuestioner.Id, withoutChildQuestioner.Id}.OrderBy(e => e).SequenceEqual(result.OrderBy(e => e)));
        }
        
        [Fact]
        public void Analyze_WithChild_And_MinKeywordMatchCount1_And_OnlyWithMaxMatch()
        {
            var settings = new BotSettings(Guid.Empty, 1)
            {
                OnlyAnswerWithMaxMatch = true
            };
            var analyzer = CreateKeywordAnalyzer(_serviceProvider, settings);

            var (rootQuestioner, childQuestioner, _) = CreateQuestioners(_appDbContext, _serviceProvider);
            
            var result = analyzer.Analyze("Как попасть на полоцкий вокзал").ToList();

            Assert.Equal(2, result.Count);
            Assert.True(new []{rootQuestioner.Id, childQuestioner.Id}.OrderBy(e => e).SequenceEqual(result.OrderBy(e => e)));
        }

        private static BotQuestioner CreateBotQuestioner(string question, BotQuestioner parent, IEnumerable<Keyword> keywords)
        {
            var res = new BotQuestioner(Guid.Empty, parent, question, null);
            var addKeywords = keywords
                .Select(kw => new BotReactionKeyword(Guid.Empty, kw, Guid.Empty, null, false));
            
            res.ReactionKeywords.AddRange(addKeywords);
            return res;
        }
        
        private static (BotQuestioner RootQuestioner, BotQuestioner ChildQuestioner, BotQuestioner WithoutChildQuestioner) 
            CreateQuestioners(AppDbContext ctx, IServiceProvider collection)
        {
            var keywordFactory = collection.GetRequiredService<KeywordFactory>();
            
            var station = keywordFactory.Create("вокзал");
            var getIn = keywordFactory.Create("попасть");
            var polotsk = keywordFactory.Create("полоцкий");
            var novopolotsk = keywordFactory.Create("новополоцкий");
            var getThere = keywordFactory.Create("доехать");
            
            var rootQuestioner = CreateBotQuestioner("Попасть на вокзал", null, new []
            {
                station,
                getIn
            });

            ctx.Questioner.Add(rootQuestioner);
            ctx.SaveChanges();
            
            var childQuestioner = CreateBotQuestioner("Как попасть на вокзал полоцка", rootQuestioner, new []
            {
                polotsk,
                station,
                getThere
            });
            
            var withoutChildQuestioner = CreateBotQuestioner("Как попасть на вокзал новополоцка", null, new []
            {
                novopolotsk,
                station,
                getThere
            });
            
            ctx.Questioner.AddRange(new []
            {
                childQuestioner,
                withoutChildQuestioner
            });
            ctx.SaveChanges();

            return (rootQuestioner, childQuestioner, withoutChildQuestioner);
        }

        private static KeywordAnalyzer CreateKeywordAnalyzer(IServiceProvider provider, BotSettings settings)
        {
            return new KeywordAnalyzer(provider,
                provider.GetRequiredService<WordLemmatizer>(),
                settings,
                provider.GetRequiredService<IMemoryCache>(),
                provider.GetRequiredService<ObjectPool<StringBuilder>>(),
                provider.GetRequiredService<KeywordFactory>());
        }
        
        public void Dispose()
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }
    }
}