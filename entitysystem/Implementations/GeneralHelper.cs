using System;
using System.Collections.Generic;

namespace Randomous.EntitySystem.Implementations
{
    public class GeneralHelper
    {
        /// <summary>
        /// Take a list of values and sort them into "buckets" based on a key. 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="keySelector"></param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public Dictionary<K,List<V>> MagicSort<K,V>(IEnumerable<V> items, Func<V,K> keySelector, Dictionary<K,List<V>> existing = null)
        {
            var result = existing ?? new Dictionary<K, List<V>>();
            foreach(var item in items)
            {
                var key = keySelector(item);
                if(!result.ContainsKey(key))
                    result.Add(key, new List<V>());
                result[key].Add(item);
            }
            return result;
        }

    }
}