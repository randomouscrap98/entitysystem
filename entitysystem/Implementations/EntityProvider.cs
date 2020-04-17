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


        public EntityProvider(ILogger<EntityProvider> logger, IEntityQueryable query,
            IEntitySearcher searcher, ISignaler<EntityBase> signaler)
        {
            this.logger = logger;
            this.query = query;
            this.searcher = searcher;
            this.signaler = signaler;
        }

        public IQueryable<T> ApplyFinal<T>(IQueryable<T> query, EntitySearchBase search) where T : EntityBase
        {
            return searcher.ApplyFinal(query, search);
        }

        public IQueryable<EntityRelation> ApplyEntityRelationSearch(IQueryable<EntityRelation> query, EntityRelationSearch search, bool finalize) {
            return searcher.ApplyEntityRelationSearch(query, search, finalize);
        }

        public IQueryable<Entity> ApplyEntitySearch(IQueryable<Entity> query, EntitySearch search, bool finalize) {
            return searcher.ApplyEntitySearch(query, search, finalize);
        }

        public IQueryable<EntityValue> ApplyEntityValueSearch(IQueryable<EntityValue> query, EntityValueSearch search, bool finalize) {
            return searcher.ApplyEntityValueSearch(query, search, finalize);
        }

        public IQueryable<T> ApplyGeneric<T>(IQueryable<T> query, EntitySearchBase search, bool finalize) where T : EntityBase {
            return searcher.ApplyGeneric<T>(query, search, finalize);
        }

        public Task<List<E>> GetAllAsync<E>() where E : EntityBase {
            return query.GetAllAsync<E>();
        }

        public Task<List<E>> GetListAsync<E>(IQueryable<E> query) {
            return this.query.GetListAsync<E>(query);
        }

        public IQueryable<E> GetQueryable<E>() where E : EntityBase {
            return query.GetQueryable<E>();
        }

        public async Task WriteAsync<E>(params E[] entities) where E : EntityBase
        {
            await query.WriteAsync(entities);
            signaler.SignalItems(entities);
        }

        public async Task DeleteAsync<E>(params E[] items) where E : EntityBase 
        {
            await query.DeleteAsync(items);
            signaler.SignalItems(items);
        }

        public Task<List<Entity>> GetEntitiesAsync(EntitySearch search)
        {
            logger.LogTrace("GetEntitiesAsync called");
            return query.GetListAsync(searcher.ApplyEntitySearch(query.GetQueryable<Entity>(), search));
        }

        public Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search)
        {
            logger.LogTrace("GetEntityRelationsAsync called");
            return query.GetListAsync(searcher.ApplyEntityRelationSearch(query.GetQueryable<EntityRelation>(), search));
        }

        public Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search)
        {
            logger.LogTrace("GetEntityValuesAsync called");
            return query.GetListAsync(searcher.ApplyEntityValueSearch(query.GetQueryable<EntityValue>(), search));
        }

        public async Task<List<E>> ListenNewAsync<E>(long lastId, TimeSpan maxWait, Func<IQueryable<E>, IQueryable<E>> filter = null) where E : EntityBase
        {
            logger.LogTrace($"ListenNewAsync called for lastId {lastId}, maxWait {maxWait}");
            filter = filter ?? new Func<IQueryable<E>, IQueryable<E>>(x => x);

            var results = await query.GetListAsync(filter(query.GetQueryable<E>().Where(x => x.id > lastId).Select(x => (E)x)));

            if(results.Count > 0)
            {
                return results;
            }
            else
            {
                var bigFilter = new Func<IQueryable<EntityBase>, IQueryable<EntityBase>>(q =>
                    filter(q.Where(e => e is E && e.id > lastId).Cast<E>())
                );

                return (await signaler.ListenAsync(bigFilter, maxWait)).Cast<E>().ToList();
            }
        }
    }
}