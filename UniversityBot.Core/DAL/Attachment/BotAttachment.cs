using System;
using Dawn;

namespace UniversityBot.Core.DAL.Attachment
{
    public abstract class BotAttachment<TImpl, TCrossEntity> : BotEntity<TImpl>
        where TImpl : BotAttachment<TImpl, TCrossEntity>
        where TCrossEntity : BotEntity
    {
        public Guid CrossEntityId { get; private set; }
        
        public TCrossEntity CrossEntity { get; private set; }
        
        public Guid FileId { get; private set; }
        
        public BotFile File { get; private set; }
   
        protected BotAttachment() : base(Guid.Empty)
        {
        }

        protected BotAttachment(Guid id, TCrossEntity crossEntity, BotFile file) : base(id)
        {
            CrossEntity = Guard.Argument(crossEntity).NotNull();
            File = Guard.Argument(file).NotNull();
            
            CrossEntityId = crossEntity.Id;
            FileId = file.Id;
            
        }

        protected override bool EqualsCore(TImpl other) 
            => FileId == other.FileId && CrossEntityId == other.CrossEntityId;

        protected override int GetHashCodeCore() 
            => HashCode.Combine(FileId, CrossEntityId);
    }
}