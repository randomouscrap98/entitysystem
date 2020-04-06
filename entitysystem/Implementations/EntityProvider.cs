using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem.Implementations
{
    public class EntityProvider : IEntityProvider
    {
        protected ILogger<EntityProvider> logger;
        protected ISignaler<EntityBase> signaler;
        protected IEntityQueryable query;
        protected IEntitySearcher searcher;
        //protected IEntityExpander expander;


        public EntityProvider(ILogger<EntityProvider> logger, IEntityQueryable query,
            IEntitySearcher searcher, /*IEntityExpander expander,*/ ISignaler<EntityBase> signaler)
        {
            this.logger = logger;
            this.query = query;
            this.searcher = searcher;
            //this.expander = expander;
            this.signaler = signaler;
        }

        public IQueryable<EntityRelation> ApplyEntityRelationSearch(IQueryable<EntityRelation> query, EntityRelationSearch search) {
            return searcher.ApplyEntityRelationSearch(query, search);
        }

        public IQueryable<Entity> ApplyEntitySearch(IQueryable<Entity> query, EntitySearch search) {
            return searcher.ApplyEntitySearch(query, search);
        }

        public IQueryable<EntityValue> ApplyEntityValueSearch(IQueryable<EntityValue> query, EntityValueSearch search) {
            return searcher.ApplyEntityValueSearch(query, search);
        }

        public IQueryable<T> ApplyGeneric<T>(IQueryable<T> query, EntitySearchBase search) where T : EntityBase {
            return searcher.ApplyGeneric<T>(query, search);
        }

        public Task DeleteAsync<E>(params E[] items) where E : EntityBase {
            return query.DeleteAsync(items);
        }

        public Task<List<E>> GetAll<E>() where E : EntityBase {
            return query.GetAll<E>();
        }

        public Task<List<E>> GetList<E>(IQueryable<E> query) {
            return this.query.GetList<E>(query);
        }

        public IQueryable<E> GetQueryable<E>() where E : EntityBase {
            return query.GetQueryable<E>();
        }

        public async Task WriteAsync<E>(params E[] entities) where E : EntityBase
        {
            await query.WriteAsync(entities);
            signaler.SignalItems(entities);
        }

        public async Task<List<Entity>> GetEntitiesAsync(EntitySearch search)
        {
            logger.LogTrace("GetEntitiesAsync called");
            return await query.GetList(searcher.ApplyEntitySearch(query.GetQueryable<Entity>(), search));
        }

        public async Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search)
        {
            logger.LogTrace("GetEntityRelationsAsync called");
            return await query.GetList(searcher.ApplyEntityRelationSearch(query.GetQueryable<EntityRelation>(), search));
        }

        public async Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search)
        {
            logger.LogTrace("GetEntityValuesAsync called");
            return await query.GetList(searcher.ApplyEntityValueSearch(query.GetQueryable<EntityValue>(), search));
        }

        public async Task<List<E>> ListenNewAsync<E>(long lastId, TimeSpan maxWait, Func<E, bool> filter = null) where E : EntityBase
        {
            logger.LogTrace($"ListenNewAsync called for lastId {lastId}, maxWait {maxWait}");
            filter = filter ?? new Func<E, bool>((x) => true);

            var results = await query.GetList(query.GetQueryable<E>().Where(x => x.id > lastId).Select(x => (E)x));
            results = results.Where(x => filter(x)).ToList(); //Maybe find a more elegant way to do this?

            if(results.Count > 0)
                return results;
            else
                return (await signaler.ListenAsync((e) => e is E && e.id > lastId && filter((E)e), maxWait)).Cast<E>().ToList();
        }
    }
}