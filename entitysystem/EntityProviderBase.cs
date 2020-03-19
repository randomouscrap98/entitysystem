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
        public abstract Task WriteAsync<E>(params E[] entities) where E : EntityBase;

        public void FinalizeWrite<E>(IEnumerable<E> items) where E : EntityBase
        {
            signaler.SignalItems(items);
        }

        public async Task WriteAsync(params EntityPackage[] entities)
        {
            foreach(var entity in entities)
            {
                logger.LogDebug($"Writing entity package for {entity.Entity}");
                await WriteAsync(entity.Entity);
                await WriteAsync(entity.Values.SelectMany(x => x.Value).ToArray());
                await WriteAsync(entity.ParentRelations.SelectMany(x => x.Value).ToArray());
            }
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

        public async Task<List<EntityPackage>> ExpandAsync(params Entity[] entities)
        {
            var results = new List<EntityPackage>();
            var ids = entities.Select(x => x.id).ToList();

            var valueSearch = new EntityValueSearch() { EntityIds = ids };
            var relationSearch = new EntityRelationSearch() {EntityIds2 = ids };

            //This is actually where the heavy lifting is.
            var allValues = await GetEntityValuesAsync(valueSearch);
            var allRelations = await GetEntityRelationsAsync(relationSearch);

            foreach(var entity in entities)
            {
                var package = new EntityPackage();
                package.Entity = entity;
                package.Values = MagicSort(allValues.Where(x => x.entityId == entity.id), new Func<EntityValue, string>((v) => v.key));
                package.ParentRelations = MagicSort(allRelations.Where(x => x.entityId2 == entity.id), new Func<EntityRelation, string>((v) => v.type));
            }

            return results;
        }
    }
}