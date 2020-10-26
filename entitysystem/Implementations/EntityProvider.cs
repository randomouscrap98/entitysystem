using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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

        //protected SemaphoreSlim accessLimiter = new SemaphoreSlim(1, 1);


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

        protected Task<T> LockAsync<T>(Func<Task<T>> func)
        {
            //await accessLimiter.WaitAsync();
            return func();
            //try { return await func(); }
            //finally { accessLimiter.Release(); }
        }
        protected Task LockAsync(Func<Task> func)
        {
            //await accessLimiter.WaitAsync();
            return func();
            //try { await func(); }
            //finally { accessLimiter.Release(); }
        }

        public Task<List<E>> GetAllAsync<E>() where E : EntityBase { 
            return LockAsync(() => query.GetAllAsync<E>()); }
        public Task<List<E>> GetListAsync<E>(IQueryable<E> query) { 
            return LockAsync(() => this.query.GetListAsync<E>(query)); }
        public Task<T> GetMaxAsync<T,E>(IQueryable<E> query, Expression<Func<E,T>> selector) { 
            return LockAsync(() => this.query.GetMaxAsync<T,E>(query, selector)); }

        public Task<IQueryable<E>> GetQueryableAsync<E>() where E : EntityBase { return query.GetQueryableAsync<E>(); }

        public async Task WriteAsync<E>(params E[] entities) where E : EntityBase
        {
            await LockAsync(() => query.WriteAsync(entities));
            signaler.SignalItems(entities);
        }

        public async Task DeleteAsync<E>(params E[] items) where E : EntityBase 
        {
            await LockAsync(() => query.DeleteAsync(items));
            signaler.SignalItems(items);
        }

        public Task<List<Entity>> GetEntitiesAsync(EntitySearch search)
        {
            logger.LogTrace("GetEntitiesAsync called");
            return LockAsync(async () => 
            {
                var entities = await query.GetQueryableAsync<Entity>();
                return await query.GetListAsync(searcher.ApplyEntitySearch(entities, search));
            });
        }

        public Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search)
        {
            logger.LogTrace("GetEntityRelationsAsync called");
            return LockAsync(async () => 
            {
                var relations = await query.GetQueryableAsync<EntityRelation>(); 
                return await query.GetListAsync(searcher.ApplyEntityRelationSearch(relations, search));
            });
        }

        public Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search)
        {
            logger.LogTrace("GetEntityValuesAsync called");
            return LockAsync(async () => 
            {
                var values = await query.GetQueryableAsync<EntityValue>();
                return await query.GetListAsync(searcher.ApplyEntityValueSearch(values, search));
            });
        }

        public async Task<List<E>> ListenAsync<E>(object listenId, Func<IQueryable<E>, IQueryable<E>> filter, TimeSpan maxWait, CancellationToken token) where E : EntityBase
        {
            logger.LogTrace($"ListenNewAsync called for maxWait {maxWait}");

            using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                //We MUST start listening FIRST so we DON'T miss anything AT ALL (we could miss valuable signals that occur while reading initially)
                var listener = signaler.ListenAsync(listenId, q => filter(q.Where(e => e is E).Cast<E>()), maxWait, linkedCts.Token);

                DateTime start = DateTime.Now; //Putting this down here to minimize startup time before listen (not that this little variable really matters)
                var baseE = await query.GetQueryableAsync<E>();
                var results = await LockAsync(() => query.GetListAsync(filter(baseE.Select(x => (E)x))));

                if (results.Count > 0)
                {
                    linkedCts.Cancel();

                    try
                    {
                        //Yes, we are so confident that we don't even worry about waiting properly
                        await listener;
                    }
                    catch(OperationCanceledException) {} //This is expected

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