
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace  Randomous.EntitySystem
{
    public interface ISignaler<T>
    {
        Dictionary<T, int> SignalItems(IEnumerable<T> items);
        Task<List<T>> ListenAsync(Func<T, bool> filter, TimeSpan maxWait);
    }
}