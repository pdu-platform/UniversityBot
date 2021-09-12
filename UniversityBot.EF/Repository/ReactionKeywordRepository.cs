using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Collections.Pooled;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Core.DAL;
using UniversityBot.EF.Extension;

namespace UniversityBot.EF.Repository
{
    public readonly struct ReactionWordResult : IEquatable<ReactionWordResult>, IDisposable
    {
        public Guid QuestionerId { get; }
        
        public PooledList<WordInfo> Words { get; }

        public ReactionWordResult(Guid questionerId, PooledList<WordInfo> words)
        {
            QuestionerId = questionerId;
            Words = words;
        }
        
        public bool Equals(ReactionWordResult other)
        {
            return QuestionerId.Equals(other.QuestionerId) && Words.SequenceEqual(other.Words);
        }

        public override bool Equals(object obj)
        {
            return obj is ReactionWordResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(QuestionerId, Words);
        }

        public void Dispose()
        {
            try
            {
                Words.Dispose();
            }
            catch
            {
                // ignore
            }
        }

        public readonly struct WordInfo : IEquatable<WordInfo>
        {
            public bool Required { get; }
            public Keyword Keyword { get; }

            public WordInfo(bool required, Keyword word)
            {
                Required = required;
                Keyword = word;
            }

            public bool Equals(WordInfo other)
            {
                return Required == other.Required && Keyword == other.Keyword;
            }

            public override bool Equals(object obj)
            {
                return obj is WordInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Required, Keyword);
            }

            public static bool operator ==(WordInfo left, WordInfo right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(WordInfo left, WordInfo right)
            {
                return !(left == right);
            }
        }

        public static bool operator ==(ReactionWordResult left, ReactionWordResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ReactionWordResult left, ReactionWordResult right)
        {
            return !(left == right);
        }
    }
    
    public class ReactionKeywordRepository
    {
        private AppDbContext _context;

        public ReactionKeywordRepository(AppDbContext context)
        {
            _context = context;
        }

        public void RemoveAll(IEnumerable<BotReactionKeyword> keywords)
        {
            _context.RemoveByTrackInfo(_context.ReactionKeywords, keywords);
        }

        public Task InsertAsync(IEnumerable<BotReactionKeyword> insertEntity, CancellationToken token = default)
        {
            return _context.ReactionKeywords.AddRangeAsync(insertEntity, token);
        }

        public PooledList<ReactionWordResult> FindByHash(ISet<ulong> words, Func<ReadOnlyMemory<char>, Keyword> keywordFactory)
        {
            return FindReactionWordByFilter(e => words.Contains(e.Hash), keywordFactory);
        }
        
        public PooledList<ReactionWordResult> FindByKeywords(ISet<string> words, Func<ReadOnlyMemory<char>, Keyword> keywordFactory)
        {
            return FindReactionWordByFilter(e => words.Contains(e.Word), keywordFactory);
        }
        
        private PooledList<ReactionWordResult> FindReactionWordByFilter(Expression<Func<BotReactionKeyword, bool>> filter, 
            Func<ReadOnlyMemory<char>, Keyword> keywordFactory)
        {
            return _context
                .ReactionKeywords
                .AsNoTracking()
                .Where(filter)
                .Select(e => new
                {
                    e.QuestionerId, 
                    e.Required, 
                    e.Word
                })
                .ToList()
                .GroupBy(e => e.QuestionerId, e => (e.Required, e.Word))
                .Select(e =>
                {
                    var questionerId = e.Key;
                    var words = e.Select(t =>
                    {
                        var keyword = keywordFactory(t.Word.AsMemory());
                        return new ReactionWordResult.WordInfo(t.Required, keyword);
                    }).ToPooledList();
                    return new ReactionWordResult(questionerId, words);
                })
                .ToPooledList();
        }
    }
}