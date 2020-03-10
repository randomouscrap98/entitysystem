using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem
{
    public abstract class EntityProviderBase
    {
        protected ILogger logger;
        protected IEntitySearcher searcher;
        protected ISignaler<EntityBase> signaler;

        protected abstract Task<List<E>> GetList<E>(IQueryable<E> query) where E : EntityBase;
        protected abstract IQueryable<E> GetQueryable<E>() where E : EntityBase;

        public void FinalizeWrite<E>(IEnumerable<E> items) where E : EntityBase
        {
            signaler.SignalItems(items);
        }

        public virtual async Task<List<Entity>> GetEntitiesAsync(EntitySearch search)
        {
            logger.LogTrace("GetEntitiesAsync called");
            return await GetList(searcher.ApplyEntitySearch(GetQueryable<Entity>(), search));
        }

        public async Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search)
        {
            logger.LogTrace("GetEntityRelationsAsync called");
            return await GetList(searcher.ApplyEntityRelationSearch(GetQueryable<EntityRelation>(), search));
        }

        public async Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search)
        {
            logger.LogTrace("GetEntityValuesAsync called");
            return await GetList(searcher.ApplyEntityValueSearch(GetQueryable<EntityValue>(), search));
        }

        public async Task<List<E>> ListenNewAsync<E>(long lastId, TimeSpan maxWait, Func<E, bool> filter = null) where E : EntityBase
        {
            logger.LogTrace($"ListenNewAsync called for lastId {lastId}, maxWait {maxWait}");
            filter = filter ?? new Func<E, bool>((x) => true);

            var results = await GetList(GetQueryable<E>().Where(x => x.id > lastId).Select(x => (E)x));
            results = results.Where(x => filter(x)).ToList(); //Maybe find a more elegant way to do this?

            if(results.Count > 0)
                return results;
            else
                return (await signaler.ListenAsync((e) => e is E && e.id > lastId && filter((E)e), maxWait)).Cast<E>().ToList();
        }
    }
}