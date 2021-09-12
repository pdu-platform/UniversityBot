using System;
using System.Collections.Generic;

namespace UniversityBot.Core.DAL
{
    public readonly struct Keyword : IEquatable<Keyword>, IComparable<Keyword>
    {
        public string Word { get; }
        public ulong Hash { get; }

        public Keyword(string word, ulong hash)
        {
            Word = word;
            Hash = hash;
        }

        public bool Equals(Keyword other) => Word == other.Word && Hash == other.Hash;

        public override bool Equals(object obj) => obj is Keyword other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Word, Hash);

        public override string ToString() => $"Keyword: {Word}; Hash: {Hash}";

        public static bool operator ==(Keyword left, Keyword right) => left.Equals(right);

        public static bool operator !=(Keyword left, Keyword right) => !(left == right);

        public int CompareTo(Keyword other)
        {
            var wordComparison = string.Compare(Word, other.Word, StringComparison.Ordinal);
            return wordComparison != 0 ? wordComparison : Hash.CompareTo(other.Hash);
        }
    }
    
    public sealed class HashComparer : IEqualityComparer<Keyword>
    {
        public static readonly HashComparer Instance = new HashComparer();
            
        public bool Equals(Keyword x, Keyword y) => x.Hash == y.Hash;

        public int GetHashCode(Keyword obj) => obj.Hash.GetHashCode();
    }
        
    public sealed class WordComparer : IEqualityComparer<Keyword>
    {
        public static readonly WordComparer Instance = new WordComparer();
            
        public bool Equals(Keyword x, Keyword y) => x.Word == y.Word;

        public int GetHashCode(Keyword obj) => obj.Word.GetHashCode();
    }
}