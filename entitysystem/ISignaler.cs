
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace  Randomous.EntitySystem
{
    public interface ISignaler<T>
    {
        Dictionary<T, int> SignalItems(IEnumerable<T> items); //, bool cancel = false);
        Task<List<T>> ListenAsync(Func<T, bool> filter, TimeSpan maxWait);
        Task<List<T>> ListenAsync(Func<IQueryable<T>, IQueryable<T>> filter, TimeSpan maxWait);
    }
}