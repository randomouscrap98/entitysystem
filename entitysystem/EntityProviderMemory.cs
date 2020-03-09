using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem
{
    public class EntityProviderMemory : EntityProviderBase, IEntityProvider
    {
        public EntityProviderMemory(ILoggerFactory logFactory) 
        {
            this.searcher = new EntitySearcher(logFactory.CreateLogger<EntitySearcher>());
            this.logger = logFactory.CreateLogger<EntityProviderMemory>();
            this.signaler = new SignalSystem<EntityBase>(logFactory.CreateLogger<SignalSystem<EntityBase>>());
        }

        public List<EntityBase> AllItems = new List<EntityBase>();

        public Task<List<Entity>> GetEntitiesAsync(EntitySearch search)
        {
            logger.LogTrace("GetEntitiesAsync called");
            return Task.FromResult(searcher.ApplyEntitySearch(AllItems.Where(x => x is Entity).Select(x => (Entity)x).AsQueryable(), search).ToList());
        }

        public Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search)
        {
            logger.LogTrace("GetEntityRelationsAsync called");
            return Task.FromResult(searcher.ApplyEntityRelationSearch(AllItems.Where(x => x is EntityRelation).Select(x => (EntityRelation)x).AsQueryable(), search).ToList());
        }

        public Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search)
        {
            logger.LogTrace("GetEntityValuesAsync called");
            return Task.FromResult(searcher.ApplyEntityValueSearch(AllItems.Where(x => x is EntityValue).Select(x => (EntityValue)x).AsQueryable(), search).ToList());
        }

        public Task DeleteAsync<E>(IEnumerable<E> items) where E : EntityBase
        {
            logger.LogTrace($"DeleteAsync called for {items.Count()} {typeof(E).Name} items");
            AllItems.RemoveAll(x => x is E && items.Any(y => y.id == x.id));
            signaler.SignalItems(items); //This is the LAST time items will get signalled!
            return Task.CompletedTask;
        }

        public virtual Task WriteAsync<E>(IEnumerable<E> items) where E : EntityBase
        {
            //Yes, we let efcore do all the work. if something weird happens... oh well. this class
            //isn't meant for safety... I think?
            logger.LogTrace($"WriteAsync called for {items.Count()} {typeof(E).Name} items");

            foreach(var item in items)
            {
                var existing = AllItems.FirstOrDefault(x => x is E && x.id == item.id);

                if(existing != null)
                {
                    item.createDate = existing.createDate;
                    AllItems.Remove(existing);
                }

                AllItems.Add(item);
            }

            signaler.SignalItems(items);
            return Task.CompletedTask;
        }

        public async Task<List<E>> ListenNewAsync<E>(long lastId, TimeSpan maxWait, Func<E, bool> filter = null) where E : EntityBase
        {
            logger.LogTrace($"ListenNewAsync called for lastId {lastId}, maxWait {maxWait}");
            filter = filter ?? new Func<E, bool>((x) => true);

            var results = AllItems.Where(x => x is E && x.id > lastId).Select(x => (E)x).ToList();
            results = results.Where(x => filter(x)).ToList(); //Maybe find a more elegant way to do this?

            if(results.Count > 0)
                return results;
            else
                return await ListenBase(lastId, filter, maxWait);
        }
    }
}