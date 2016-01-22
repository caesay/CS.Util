using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace CS.Util.Collections
{
    /// <summary>
    /// Provides a wrapper around the .Net MemoryCache that provides type safety for the value.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to be stored.</typeparam>
    public class GenericMemoryCache<TValue> : IGenericCache<string, TValue>
    {
        public long Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _internal.GetCount();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        private bool _disposed;
        private MemoryCache _internal;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public GenericMemoryCache()
        {
            _internal = new MemoryCache(typeof(TValue).Name + "_" + RandomEx.GetString(8));
        }
        ~GenericMemoryCache()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                ((IDisposable)_internal).Dispose();
                ((IDisposable)_lock).Dispose();
            }
        }

        public TValue Get(string key)
        {
            _lock.EnterReadLock();
            try
            {
                return (TValue)_internal.Get(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        public IDictionary<string, TValue> GetRange(IEnumerable<string> keys)
        {
            _lock.EnterReadLock();
            try
            {
                return _internal.GetValues(keys)?.ToDictionary(kvp => kvp.Key, kvp => (TValue)kvp.Value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Add(GenericCacheItem<string, TValue> item, CacheItemPolicy policy)
        {
            Add(item.Key, item.Value, policy);
        }
        public void Add(string key, TValue value, CacheItemPolicy policy)
        {
            _lock.EnterWriteLock();
            try
            {
                _internal.Add(key, value, policy);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public void Set(GenericCacheItem<string, TValue> item, CacheItemPolicy policy)
        {
            Set(item.Key, item.Value, policy);
        }
        public void Set(string key, TValue value, CacheItemPolicy policy)
        {
            _lock.EnterWriteLock();
            try
            {
                _internal.Set(key, value, policy);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public TValue Delete(string key)
        {
            _lock.EnterWriteLock();
            try
            {
                return (TValue)_internal.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _internal.Dispose();
                _internal = new MemoryCache(typeof(TValue).Name + "_" + Guid.NewGuid());
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _internal.Select(kvp => new KeyValuePair<string, TValue>(kvp.Key, (TValue)kvp.Value)).GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool ContainsKey(string key)
        {
            _lock.EnterReadLock();
            try
            {
                return _internal.Contains(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// A wrapper around the .Net MemoryCache that provides type safety for the value, and also supports 
    /// a non-string key type. This implementation stores an additional WeakReference for each cache object
    /// so its slower and has a higher overhead than the standard <see cref="GenericMemoryCache{T}" />
    /// or <see cref="MemoryCache"/>
    /// </summary>
    /// <typeparam name="TKey">The cache lookup key</typeparam>
    /// <typeparam name="TValue">The type of the value to be stored</typeparam>
    public class GenericMemoryCache<TKey, TValue> : IGenericCache<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        public long Count
        {
            get { return _internal.Count; }
        }

        private readonly ConditionalWeakTable<TKey, string> _weakTable;
        private readonly GenericMemoryCache<GenericCacheItem<TKey, TValue>> _internal;

        public GenericMemoryCache()
        {
            _weakTable = new ConditionalWeakTable<TKey, string>();
            _internal = new GenericMemoryCache<GenericCacheItem<TKey, TValue>>();
        }

        ~GenericMemoryCache()
        {
            Dispose();
        }
        public void Dispose()
        {
            ((IDisposable)_internal).Dispose();
        }

        public TValue Get(TKey key)
        {
            string token;
            bool exists = _weakTable.TryGetValue(key, out token);

            return exists
                ? _internal.Get(token)?.Value
                : null;
        }
        IDictionary<TKey, TValue> IGenericCache<TKey, TValue>.GetRange(IEnumerable<TKey> keys)
        {
            throw new NotImplementedException();
        }

        public void Add(GenericCacheItem<TKey, TValue> item, CacheItemPolicy policy)
        {
            string unused;
            if (_weakTable.TryGetValue(item.Key, out unused))
            {
                if (_internal.ContainsKey(unused))
                    throw new InvalidOperationException("Provided object key already exists in this cache.");
                else
                    _weakTable.Remove(item.Key);
            }

            string token = Guid.NewGuid().ToString();
            _weakTable.Add(item.Key, token);
            _internal.Add(token, item, policy);
        }
        public void Add(TKey key, TValue value, CacheItemPolicy policy)
        {
            Add(new GenericCacheItem<TKey, TValue>(key, value), policy);
        }
        public void Set(GenericCacheItem<TKey, TValue> item, CacheItemPolicy policy)
        {
            string token;
            bool exists = _weakTable.TryGetValue(item.Key, out token);
            if (!exists)
            {
                token = new Guid().ToString();
                _weakTable.Add(item.Key, token);
            }
            _internal.Set(token, item, policy);
        }
        public void Set(TKey key, TValue value, CacheItemPolicy policy)
        {
            Set(new GenericCacheItem<TKey, TValue>(key, value), policy);
        }

        public TValue Delete(TKey key)
        {
            string token;
            bool exists = _weakTable.TryGetValue(key, out token);
            if (exists)
            {
                _weakTable.Remove(key);
                return _internal.Delete(token)?.Value;
            }
            return null;
        }
        public void Clear()
        {
            _internal.Select(k => k.Value?.Key).ToList().ForEach(k => _weakTable.Remove(k));
            _internal.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _internal.Select(kvp => new KeyValuePair<TKey, TValue>(kvp.Value.Key, kvp.Value.Value)).GetEnumerator();
        }

        public bool ContainsKey(TKey key)
        {
            string token;
            return _weakTable.TryGetValue(key, out token) && _internal.ContainsKey(token);
        }
    }

    public class GenericCacheItem<TKey, TValue>
    {
        public TKey Key { get; }
        public TValue Value { get; }

        public GenericCacheItem(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
    public interface IGenericCache<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        long Count { get; }
        void Add(GenericCacheItem<TKey, TValue> item, CacheItemPolicy policy);
        void Add(TKey key, TValue value, CacheItemPolicy policy);
        void Clear();
        bool ContainsKey(TKey key);
        TValue Delete(TKey key);
        TValue Get(TKey key);
        IDictionary<TKey, TValue> GetRange(IEnumerable<TKey> keys);
        void Set(GenericCacheItem<TKey, TValue> item, CacheItemPolicy policy);
        void Set(TKey key, TValue value, CacheItemPolicy policy);
    }
}