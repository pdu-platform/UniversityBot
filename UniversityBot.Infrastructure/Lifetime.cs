using System;
using System.Collections.Generic;
using System.Linq;
using Collections.Pooled;

namespace UniversityBot.Infrastructure
{
    public struct Lifetime : IDisposable
    {
        private PooledList<IDisposable> _store;

        public Lifetime Add<T>(T obj)
            where T : IDisposable
        {
            EnsureInit();
            _store.Add(obj);
            return this;
        }

        public Lifetime AddRange<T>(IEnumerable<T> seq)
            where T : IDisposable
        {
            EnsureInit();
            _store.AddRange(seq.Cast<IDisposable>());
            return this;
        }
        
        public void Dispose()
        {
            if(_store == null)
                return;
            
            for (var i = _store.Count - 1; i >= 0; i--)
            {
                try
                {
                    _store[i].Dispose();
                }
                catch
                {
                    // ignore
                }
            }
        }

        private void EnsureInit()
        {
            if(_store != null)
                return;
            _store = new PooledList<IDisposable>();
        }
    }
}