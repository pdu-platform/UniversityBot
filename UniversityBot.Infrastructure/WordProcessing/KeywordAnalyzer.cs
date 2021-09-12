using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Collections.Pooled;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using UniversityBot.Core.DAL;
using UniversityBot.Core.Util;
using UniversityBot.EF;
using UniversityBot.EF.Repository;

namespace UniversityBot.Infrastructure.WordProcessing
{
    public class KeywordAnalyzer
    {
        private readonly uint _minMatchQuantity;
        private readonly bool _dontDisplayChildAndParentQuestionInAnswer;
        private readonly bool _onlyAnswerWithMaxMatch;
        private readonly bool _useFuzzySearch;

        private readonly IServiceProvider _serviceProvider;
        private readonly WordLemmatizer _wordLemmatizer;
        private readonly IMemoryCache _cache;
        private readonly ObjectPool<StringBuilder> _stringBuilderPool;
        private readonly KeywordFactory _keywordFactory;

        public KeywordAnalyzer(IServiceProvider serviceProvider, WordLemmatizer wordLemmatizer, BotSettings settings,
            IMemoryCache cache, ObjectPool<StringBuilder> stringBuilderPool, KeywordFactory keywordFactory)
        {
            _serviceProvider = serviceProvider;
            _wordLemmatizer = wordLemmatizer;
            _cache = cache;
            _stringBuilderPool = stringBuilderPool;
            _keywordFactory = keywordFactory;

            _useFuzzySearch = settings.UseFuzzySearch;
            _minMatchQuantity = settings.MinKeywordMatchCount;
            _dontDisplayChildAndParentQuestionInAnswer = settings.DontDisplayChildAndParentQuestionInAnswer;
            _onlyAnswerWithMaxMatch = settings.OnlyAnswerWithMaxMatch;
        }
        
        public IEnumerable<Guid> Analyze(string sentence)
        {
            using var uniqKeywords = ToKeywords(sentence)
                .Where(e => e.Word.Length > 3)
                .ToPooledSet(_useFuzzySearch ? HashComparer.Instance : WordComparer.Instance);
            
            if (uniqKeywords.Count == 0)
                yield break;

            using var lifetime = new Lifetime();
            using var scope = _serviceProvider.CreateScope();

            var reactionKeywordRep = scope.ServiceProvider.GetRequiredService<ReactionKeywordRepository>();
            using var reactionWordsInfo = _useFuzzySearch
                ? reactionKeywordRep.FindByHash(uniqKeywords.Select(e => e.Hash).ToHashSet(), _keywordFactory.Create)
                : reactionKeywordRep.FindByKeywords(uniqKeywords.Select(e => e.Word).ToHashSet(), _keywordFactory.Create);

            var (matchQuantityStore, maxMatchQuantity) = BuildMatchQuantityInfo(reactionWordsInfo, uniqKeywords);
            lifetime.Add(matchQuantityStore).AddRange(reactionWordsInfo);

            var filterAnswerEnumerable = FilterAnswer(matchQuantityStore, maxMatchQuantity);

            using var filterAnswers = filterAnswerEnumerable.ToPooledList();

            if (_dontDisplayChildAndParentQuestionInAnswer)
                RemoveAllParent(filterAnswers, tuple => tuple.QuestionerId, scope.ServiceProvider);

            foreach (var (answerId, _) in filterAnswers)
                yield return answerId;
        }

        private IEnumerable<(Guid QuestionerId, int MatchQuantity)> FilterAnswer(PooledList<(Guid QuestionerId, int MatchQuantity)> matchQuantityStore, int maxMatchQuantity)
        {
            return _onlyAnswerWithMaxMatch
                ? matchQuantityStore.Where(e => e.MatchQuantity == maxMatchQuantity)
                : matchQuantityStore.Where(e => e.MatchQuantity >= _minMatchQuantity);
        }

        private void RemoveAllParent<T>(PooledList<T> reactionWordsInfo, Func<T, Guid> questionerIdSelector,
            IServiceProvider provider)
        {
            var ctx = provider.GetRequiredService<AppDbContext>();

            using var checkItems = reactionWordsInfo.Select(questionerIdSelector).ToPooledList();
            using var removeItems = GetItemsForRemove(checkItems, ctx);
            reactionWordsInfo.RemoveAll(item =>
            {
                var questionerId = questionerIdSelector(item);
                return removeItems.Contains(questionerId);
            });
        }

        private static (PooledList<(Guid QuestionerId, int MatchQuantity)>, int MaxMatchQuantity)
            BuildMatchQuantityInfo<T>(IEnumerable<ReactionWordResult> reactionWordsInfo, T uniqKeywords)
            where T : ISet<Keyword>
        {
            var matchQuantityStore = new PooledList<(Guid, int)>();
            var maxMatchQuantity = 0;

            foreach (var reactionWordResult in reactionWordsInfo)
            {
                var (questionerId, words) = (reactionWordResult.QuestionerId, reactionWordResult.Words);
                var matchQuantity = 0;

                var requiredCount = words.Count(e => e.Required);
                if (requiredCount > 0)
                {
                    if (!words.Where(e => e.Required).All(word => uniqKeywords.Contains(word.Keyword)))
                        continue;

                    matchQuantity += requiredCount;
                }

                matchQuantity += words.Count(e => !e.Required && uniqKeywords.Contains(e.Keyword));

                matchQuantityStore.Add((questionerId, matchQuantity));

                if (maxMatchQuantity < matchQuantity)
                    maxMatchQuantity = matchQuantity;
            }

            return (matchQuantityStore, maxMatchQuantity);
        }

        private PooledSet<Guid> GetItemsForRemove(IReadOnlyList<Guid> reactionWordsInfo, AppDbContext ctx)
        {
            return reactionWordsInfo
                .Where(questionerId =>
                {
                    var checkChild = reactionWordsInfo
                        .Where(id => id != questionerId)
                        .Select(id => id);

                    return AnyChild(questionerId, ctx, checkChild);
                })
                .ToPooledSet();
        }

        private IEnumerable<Keyword> ToKeywords(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                yield break;

            var sb = _stringBuilderPool.Get();
            try
            {
                var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var t in words)
                {
                    var keyword = ToKeyword(t, sb.Clear());
                    if (!keyword.HasValue)
                        continue;

                    yield return keyword.Value;
                }
            }
            finally
            {
                _stringBuilderPool.Return(sb);
            }
        }

        private Keyword? ToKeyword(string word, StringBuilder sb)
        {
            if (string.IsNullOrWhiteSpace(word))
                return null;

            foreach (var character in word)
                sb.Append(char.IsPunctuation(character) ? ' ' : char.ToLower(character));

            var lem = _wordLemmatizer.Lemmatize(sb.Trim().ToString());
            return _keywordFactory.Create(lem.AsMemory());
        }

        private bool AnyChild(Guid id, AppDbContext context, IEnumerable<Guid> filterEntity)
        {
            object key = id;
            if (_cache.TryGetValue(key, out bool anyChild))
                return anyChild;

            anyChild = context.Questioner
                .AsNoTracking()
                .Any(e => e.ParentId == id && filterEntity.Contains(e.Id));

            var entry = _cache.CreateEntry(key);
            entry.Value = anyChild;
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            entry.Dispose();

            return anyChild;
        }
    }
}