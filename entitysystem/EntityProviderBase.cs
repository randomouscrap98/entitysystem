using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem
{
    public class EntityProviderBase
    {
        protected ILogger logger;
        protected IEntitySearcher searcher;
        protected ISignaler<EntityBase> signaler;

        public async Task<List<E>> ListenBase<E>(long lastId, Func<E,bool> filter, TimeSpan maxWait) where E : EntityBase
        {
            return (await signaler.ListenAsync((e) => e is E && e.id > lastId && filter((E)e), maxWait)).Cast<E>().ToList();
        }

        public void FinalizeWrite<E>(IEnumerable<E> items) where E : EntityBase
        {
            signaler.SignalItems(items);
        }
    }
}