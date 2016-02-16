using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Collections
{
    /// <summary>
    /// Provides a managed array of the specified size, and guarentees that the data will not be duplicated in memory.
    /// The memory is cleared when the array is disposed. It is designed to be a short-lived class for sensitive data that
    /// needs to be removed from memory quickly and will not allow the GC to move the data for the lifetime of the class.
    /// </summary>
    /// <typeparam name="T">The array type.</typeparam>
    public class PinnedArray<T> : IList<T>, IDisposable
        where T : struct
    {
        public IntPtr Handle
        {
            get
            {
                ThrowIfDisposed();
                return _handle.AddrOfPinnedObject();
            }
        }

        private T[] _arr;
        private GCHandle _handle;
        private bool _disposed;

        public PinnedArray(int count)
        {
            _arr = new T[count];
            _handle = GCHandle.Alloc(_arr, GCHandleType.Pinned);
        }

        ~PinnedArray()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            for (var i = 0; i < _arr.Length; i++)
                _arr[i] = default(T);
            _handle.Free();
            GC.SuppressFinalize(this);
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("PinnedArray<T>");
        }

        #region IList<T>
        public IEnumerator<T> GetEnumerator()
        {
            ThrowIfDisposed();
            return ((IEnumerable<T>)_arr).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowIfDisposed();
            return _arr.GetEnumerator();
        }
        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }
        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }
        public bool Contains(T item)
        {
            ThrowIfDisposed();
            return _arr.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            ThrowIfDisposed();
            _arr.CopyTo(array, arrayIndex);
        }
        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }
        public int Count
        {
            get
            {
                ThrowIfDisposed();
                return _arr.Length;
            }
        }
        public bool IsReadOnly
        {
            get
            {
                ThrowIfDisposed();
                return _arr.IsReadOnly;
            }
        }
        public int IndexOf(T item)
        {
            ThrowIfDisposed();
            return Array.IndexOf(_arr, item);
        }
        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }
        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
        public T this[int index]
        {
            get
            {
                ThrowIfDisposed();
                return _arr[index];
            }
            set
            {
                ThrowIfDisposed();
                _arr[index] = value;
            }
        }
        #endregion
    }
}
