using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem
{
    public class EntityProviderEfCoreConfig
    {
        public int MaxRetrieve {get;set;} = 10000;
    }

    public class EntityProviderEfCore : IEntityProvider
    {
        protected ILogger logger;
        protected IEntitySearcher searcher;
        protected ISignaler<EntityBase> signaler;

        public DbContext context;
        public EntityProviderEfCoreConfig config;

        public EntityProviderEfCore(ILogger<EntityProviderEfCore> logger, IEntitySearcher searcher, 
            DbContext context, ISignaler<EntityBase> signaler, EntityProviderEfCoreConfig config)
        {
            this.searcher = searcher;
            this.logger = logger;
            this.context = context;
            this.config = config;
            this.signaler = signaler;
        }

        public async Task<List<Entity>> GetEntitiesAsync(EntitySearch search)
        {
            logger.LogTrace("GetEntitiesAsync called");
            return await searcher.ApplyEntitySearch(context.Set<Entity>(), search).Take(config.MaxRetrieve).ToListAsync();
        }

        public async Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search)
        {
            logger.LogTrace("GetEntityRelationsAsync called");
            return await searcher.ApplyEntityRelationSearch(context.Set<EntityRelation>(), search).Take(config.MaxRetrieve).ToListAsync();
        }

        public async Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search)
        {
            logger.LogTrace("GetEntityValuesAsync called");
            return await searcher.ApplyEntityValueSearch(context.Set<EntityValue>(), search).Take(config.MaxRetrieve).ToListAsync();
        }

        public async virtual Task WriteAsync<E>(IEnumerable<E> items) where E : EntityBase
        {
            //Yes, we let efcore do all the work. if something weird happens... oh well. this class
            //isn't meant for safety... I think?
            logger.LogTrace($"WriteAsync called for {items.Count()} {typeof(E).Name} items");
            context.UpdateRange(items);
            await context.SaveChangesAsync();
            signaler.SignalItems(items);
        }

        public async Task<List<E>> ListenNewAsync<E>(long lastId, TimeSpan maxWait, Func<E, bool> filter = null) where E : EntityBase
        {
            logger.LogTrace($"ListenNewAsync called for lastId {lastId}, maxWait {maxWait}");
            filter = filter ?? new Func<E, bool>((x) => true);

            var results = await context.Set<E>().Where(x => x.id > lastId).Take(config.MaxRetrieve).ToListAsync();
            results = results.Where(x => filter(x)).ToList(); //Maybe find a more elegant way to do this?

            if(results.Count > 0)
                return results;
            else
                return (await signaler.ListenAsync((e) => e is E && e.id > lastId && filter((E)e), maxWait)).Cast<E>().ToList();
        }
    }
}