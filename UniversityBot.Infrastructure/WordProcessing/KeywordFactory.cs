using System;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using UniversityBot.Core.DAL;
using UniversityBot.Infrastructure.Extension;

namespace UniversityBot.Infrastructure.WordProcessing
{
    public sealed class KeywordFactory
    {
        private readonly HashService _hashService;
        private readonly ObjectPool<StringBuilder> _objectPool;
        private readonly WordLemmatizer _wordLemmatizer;

        public KeywordFactory(HashService hashService, ObjectPool<StringBuilder> objectPool, WordLemmatizer wordLemmatizer)
        {
            _hashService = hashService;
            _objectPool = objectPool;
            _wordLemmatizer = wordLemmatizer;
        }

        public Keyword Create(string keyword) => Create(keyword.AsMemory());
        
        public Keyword Create(ReadOnlyMemory<char> keyword)
        {
            var normalizedKeyword = ToKeyword(keyword);
            
            var wordForHashing = BuildWordToHash(normalizedKeyword.AsMemory()).ToString();
            
            var keywordHash = _hashService.ComputeHash(wordForHashing);

            return new Keyword(normalizedKeyword, keywordHash);
        }
        
        private string ToKeyword(ReadOnlyMemory<char> word)
        {
            if (word.IsEmpty)
                return string.Empty;

            var trimWord = word.Trim();
            
            // Больше 1 слова
            var idxSpace = trimWord.Span.IndexOf(' ');
            if (idxSpace > 0)
                trimWord = trimWord[..idxSpace];

            var lowerWord = string.Create(trimWord.Length, trimWord, (t, ctx) =>
            {
                var idx = 0;
                foreach (var c in ctx.Span)
                {
                    t[idx] = char.ToLower(c);
                    idx += 1;
                }
            });
 
            return _wordLemmatizer.Lemmatize(lowerWord);
        }

        private ReadOnlyMemory<char> BuildWordToHash(ReadOnlyMemory<char> word)
        {
            if (word.Length <= 0) 
                return word;
            
            using var item = _objectPool.GetScoped();
            var sb = item.Item.Append(word);
            
            var lastChar = word.Span[^1];
            
            // Считаем что длина введенного слова = 37%. т.е нужно расширить его длину на (100% / 37%) * 37%
            var appendCount =  (int)Math.Ceiling(word.Length * 2.7);
            
            for (var i = 0; i < appendCount; i++)
                sb.Append(lastChar);

            return sb.ToString().AsMemory();

        }
    }
}