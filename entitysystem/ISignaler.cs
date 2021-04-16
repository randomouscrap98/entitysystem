using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace  Randomous.EntitySystem
{
    public class ListenerData
    {
        public object ListenerId;
        public DateTime StartedListening;
    }

    public interface ISignaler<T>
    {
        Dictionary<T, int> SignalItems(IEnumerable<T> items);
        Task<List<T>> ListenAsync(object listenerId, Func<T, bool> filter, TimeSpan maxWait, CancellationToken token);
        Task<List<T>> ListenAsync(object listenerId, Func<IQueryable<T>, IQueryable<T>> filter, TimeSpan maxWait, CancellationToken token);

        List<ListenerData> Listeners {get;}
    }
}