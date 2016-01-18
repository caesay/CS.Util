using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using System.Threading;

namespace CS.Util.Collections
{
    /// <summary>
    /// Provides a wrapper around the .Net MemoryCache that provides type safety for the value only.
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
    /// a non-string key type. The key object should implement IEquatable, or a custom IEqualityComparer should be
    /// specified in the constructor.
    /// </summary>
    /// <typeparam name="TKey">The cache lookup key, should implement GetHashCode and/or implement IEquatable</typeparam>
    /// <typeparam name="TValue">The type of the value to be stored</typeparam>
    public class GenericMemoryCache<TKey, TValue> : IGenericCache<TKey, TValue>
    {
        public long Count
        {
            get { return _internal.Count; }
        }

        private readonly IEqualityComparer<TKey> _comparer;
        private readonly GenericMemoryCache<GenericCacheItem<TKey, TValue>> _internal;

        public GenericMemoryCache()
            :this(EqualityComparer<TKey>.Default)
        {
        }
        public GenericMemoryCache(IEqualityComparer<TKey> comparer)
        {
            _comparer = comparer;
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
            string skey = _comparer.GetHashCode(key).ToString();
            return _internal.Get(skey).Value;
        }
        public IDictionary<TKey, TValue> GetRange(IEnumerable<TKey> keys)
        {
            return _internal.GetRange(keys.Select(s => _comparer.GetHashCode(s).ToString()))
                .ToDictionary(kvp => kvp.Value.Key, kvp => kvp.Value.Value);
        }

        public void Add(GenericCacheItem<TKey, TValue> item, CacheItemPolicy policy)
        {
            _internal.Add(_comparer.GetHashCode(item.Key).ToString(), item, policy);
        }
        public void Add(TKey key, TValue value, CacheItemPolicy policy)
        {
            Add(new GenericCacheItem<TKey, TValue>(key, value), policy);
        }
        public void Set(GenericCacheItem<TKey, TValue> item, CacheItemPolicy policy)
        {
            _internal.Set(_comparer.GetHashCode(item.Key).ToString(), item, policy);
        }
        public void Set(TKey key, TValue value, CacheItemPolicy policy)
        {
            Set(new GenericCacheItem<TKey, TValue>(key, value), policy);
        }

        public TValue Delete(TKey key)
        {
            return _internal.Delete(_comparer.GetHashCode(key).ToString()).Value;
        }
        public void Clear()
        {
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
            return _internal.ContainsKey(_comparer.GetHashCode(key).ToString());
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