using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Collections
{
    /// <summary>
    /// Wraps <![CDATA[List<KeyValuePair<TKey, TValue>]]>, providing some helper methods to make it less painful.
    /// </summary>
    public class ListKvp<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
    {
        public IEnumerable<TKey> Keys => this.Select(kvp => kvp.Key); 
        public IEnumerable<TValue> Values => this.Select(kvp => kvp.Value);

        public IEnumerable<KeyValuePair<TKey, TValue>> this[TKey key]
        {
            get { return this.Where(kvp => kvp.Key.Equals(key)); }
        }

        public void Add(TKey key, TValue value)
        {
            var element = new KeyValuePair<TKey, TValue>(key, value);
            this.Add(element);
        }
        public void RemoveByKey(TKey key)
        {
            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (this[i].Key.Equals(key))
                {
                    this.RemoveAt(i);
                }
            }
        }
        public void RemoveByValue(TValue value)
        {
            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (this[i].Value.Equals(value))
                {
                    this.RemoveAt(i);
                }
            }
        }
        public bool ContainsKey(TKey key)
        {
            return this.Any(kvp => kvp.Key.Equals(key));
        }
    }
}
