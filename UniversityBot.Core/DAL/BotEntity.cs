using System;
using Newtonsoft.Json;

namespace UniversityBot.Core.DAL
{
    public abstract class BotEntity : IEquatable<BotEntity>, IBotEntity
    {
        public Guid Id { get; protected set; }

        protected BotEntity(Guid id)
        {
            Id = id;
        }
        
        public abstract bool Equals(BotEntity other);

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, GetType(), new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            });
        }
    }
}