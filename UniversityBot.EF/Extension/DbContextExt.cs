using System;
using System.Collections.Generic;
using System.Linq;
using Collections.Pooled;
using Dawn;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Core.DAL;

namespace UniversityBot.EF.Extension
{
    public static class DbContextExt
    {
        public static PooledDictionary<Guid, (TVal Entity, bool IsTracked)> GetTrackInfo<TVal>
            (this DbContext self, PooledDictionary<Guid, TVal> map) 
            where TVal : class, IBotEntity
        {
            var notTracked = map.ToPooledDictionary(e => e.Key, e => (e.Value, IsTracked: false));
            
            foreach (var entityEntry in self.ChangeTracker.Entries<TVal>())
            {
                var entry = entityEntry.Entity;

                if (map.TryGetValue(entry.Id, out _))
                    notTracked[entry.Id] = (entry, true);
            }

            return notTracked;
        }
        
        private static void AttachNotTracked<TVal>(this DbContext self, PooledDictionary<Guid, TVal> entityForTrack,
            DbSet<TVal> set)
            where TVal : class, IBotEntity
        {
            using var trackInfo = self.GetTrackInfo(entityForTrack);
            
            var notTrackedEntity = trackInfo.Select(e => e.Value)
                .Where(e => !e.IsTracked)
                .Select(e => e.Entity);
            
            set.AttachRange(notTrackedEntity);
        }

        public static void UpdateByTrackInfo<TVal>(this DbContext self, DbSet<TVal> set, IEnumerable<TVal> entities)
            where TVal : class, IBotEntity
        {
            Guard.Argument(entities, nameof(entities)).NotNull();
            
            using var polledEntityForTrack = entities.ToPooledDictionary(e => e.Id);
            
            self.AttachNotTracked(polledEntityForTrack, set);
            
            set.UpdateRange(polledEntityForTrack.Values);
        }
        
        public static void RemoveByTrackInfo<TVal>(this DbContext self, DbSet<TVal> set, PooledDictionary<Guid, TVal> entityForTrack)
            where TVal : class, IBotEntity
        {
            if(entityForTrack.Count == 0)
                return;

            self.AttachNotTracked(entityForTrack, set);

            var removeEntity = entityForTrack.Select(e => e.Value);
            set.RemoveRange(removeEntity);
        }
        
        public static void RemoveByTrackInfo<TCtx, TVal>(this TCtx self, DbSet<TVal> set, IEnumerable<TVal> entityForTrack)
            where TVal : class, IBotEntity
            where TCtx : DbContext
        {
            if (entityForTrack == null)
                throw new ArgumentNullException(nameof(entityForTrack));
                   
            using var polledEntityForTrack = entityForTrack.ToPooledDictionary(e => e.Id);
            self.RemoveByTrackInfo(set, polledEntityForTrack);
        }
        
        public static void RemoveByTrackInfo<TCtx, TVal>(this TCtx self, DbSet<TVal> set, params TVal[] entityForTrack)
            where TVal : class, IBotEntity
            where TCtx : DbContext
        {
            if (entityForTrack == null)
                throw new ArgumentNullException(nameof(entityForTrack));
     
            self.RemoveByTrackInfo(set, (IEnumerable<TVal>)entityForTrack);
        }
    }
}