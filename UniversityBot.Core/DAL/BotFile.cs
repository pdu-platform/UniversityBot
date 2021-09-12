using System;
using System.IO;
using Dawn;
using UniversityBot.Core.Util;

namespace UniversityBot.Core.DAL
{
    public class BotFile : BotEntity<BotFile>
    {
        public byte[] Content { get; private set; }
        
        public long Size { get; private set; }
        
        public string Name { get; private set; }
        
        public string Extension { get; private set; }
        
        private BotFile() : base(Guid.Empty)
        {
        }
        
        public BotFile(Guid id) : base(id)
        {
        }

        public BotFile(Guid id, byte[] content, long size, string name, string extension) 
            : base(id)
        {
            Content = Guard.Argument(content).NotNull().Value;
            Size = Guard.Argument(size).GreaterThan(0);
            Name = Guard.Argument(name).NotNull().NotWhiteSpace();
            
            Name = Path.GetFileNameWithoutExtension(name);
            
            Extension = Guard.Argument(extension).NotNull().NotWhiteSpace().StartsWith(".");
        }
        
        protected override bool EqualsCore(BotFile other)
        {
            return Size == other.Size &&
                   Name == other.Name &&
                   Extension == other.Extension &&
                   CollectionsUtil.AreSame(Content, other.Content);
        }

        protected override int GetHashCodeCore() =>
            HashCode.Combine(Size, Name, Extension, CollectionsUtil.HashCode(Content));
    }
}