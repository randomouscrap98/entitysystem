using System;
using System.Collections.Generic;

namespace Randomous.EntitySystem
{
    public class GeneralHelper
    {
        public Dictionary<K,List<V>> MagicSort<K,V>(IEnumerable<V> items, Func<V,K> keySelector)
        {
            var result = new Dictionary<K, List<V>>();
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