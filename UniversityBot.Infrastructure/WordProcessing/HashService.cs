using System.Text;
using Microsoft.Extensions.ObjectPool;
using SimhashLib;
using UniversityBot.Infrastructure.Extension;

namespace UniversityBot.Infrastructure.WordProcessing
{
    public sealed class HashService
    {
        private readonly ObjectPool<StringBuilder> _objPool;

        public HashService(ObjectPool<StringBuilder> objPool)
        {
            _objPool = objPool;
        }
        
        public ulong ComputeHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;
            
            var simhash = new Simhash();

            using var sbCookie = _objPool.GetScoped();
            
            var tokens = Shingling.Tokenize(input, sbCookie);
            return simhash.ComputeHashByMurmurHash3(tokens).Value;
        }
    }
}