using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem.Implementations
{
    public class EntityProviderConfig
    {
        public TimeSpan DualListenCancelMinimum = TimeSpan.FromMilliseconds(3);
    }

    public class EntityProvider : IEntityProvider
    {
        protected ILogger<EntityProvider> logger;
        protected ISignaler<EntityBase> signaler;
        protected IEntityQueryable query;
        protected IEntitySearcher searcher;
        protected EntityProviderConfig config;


        public EntityProvider(ILogger<EntityProvider> logger, IEntityQueryable query,
            IEntitySearcher searcher, ISignaler<EntityBase> signaler, EntityProviderConfig config)
        {
            this.logger = logger;
            this.query = query;
            this.searcher = searcher;
            this.signaler = signaler;
            this.config = config;
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

        public async Task<List<E>> ListenAsync<E>(object listenId, Func<IQueryable<E>, IQueryable<E>> filter, TimeSpan maxWait, CancellationToken token) where E : EntityBase
        {
            logger.LogTrace($"ListenNewAsync called for maxWait {maxWait}");

            using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                //We MUST start listening FIRST so we DON'T miss anything AT ALL (we could miss valuable signals that occur while reading initially)
                var listener = signaler.ListenAsync(listenId, q => filter(q.Where(e => e is E).Cast<E>()), maxWait, linkedCts.Token);

                DateTime start = DateTime.Now; //Putting this down here to minimize startup time before listen (not that this little variable really matters)
                var results = await query.GetListAsync(filter(query.GetQueryable<E>().Select(x => (E)x)));

                if (results.Count > 0)
                {
                    linkedCts.Cancel();

                    if(!listener.Wait((int)Math.Max((maxWait - (DateTime.Now - start)).TotalMilliseconds, config.DualListenCancelMinimum.TotalMilliseconds)))
                    {
                        logger.LogWarning("Pre-emptive listener did not cancel in time! Attempting to dispose, this could throw!!");

                        try { listener.Dispose(); }
                        catch(Exception ex) { logger.LogError($"EXCEPTION WHILE DISPOSING LISTENER: {ex}"); }
                    }

                    return results;
                }
                else
                {
                    return (await listener).Cast<E>().ToList();
                }
            }
        }

        public List<ListenerData> Listeners => signaler.Listeners;
    }
}