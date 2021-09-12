using System;

namespace UniversityBot.Core.DAL
{
    public abstract class BotEntity<T> : BotEntity, IEquatable<T>
        where T : BotEntity<T>
    {
        public BotEntity(Guid id) : base(id)
        {
            
        }
        
        public bool Equals(T other)
        {
            if (ReferenceEquals(other, null))
                return false;
            
            if (ReferenceEquals(this, other))
                return false;

            if (GetType() != other.GetType())
                return false;

            return Id == other.Id && EqualsCore(other);
        }

        public override bool Equals(object obj) => obj is T o && Equals(o);

        public override bool Equals(BotEntity other) => other is T o && Equals(o);

        public override int GetHashCode() => HashCode.Combine(Id, GetHashCodeCore());

        protected abstract bool EqualsCore(T other);
        protected abstract int GetHashCodeCore();
    }
}