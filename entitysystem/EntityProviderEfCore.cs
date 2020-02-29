using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace entitysystem
{
    public class EntityProviderEfCoreConfig
    {
        public int MaxRetrieve {get;set;} = 10000;
    }

    public class EntityProviderEfCore : IEntityProvider
    {
        protected ILogger logger;
        protected IEntitySearcher searcher;
        public DbContext context;
        public EntityProviderEfCoreConfig config;

        public EntityProviderEfCore(ILogger<EntityProviderEfCore> logger, IEntitySearcher searcher, DbContext context, EntityProviderEfCoreConfig config)
        {
            this.searcher = searcher;
            this.logger = logger;
            this.context = context;
            this.config = config;
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
            context.UpdateRange(items);
            await context.SaveChangesAsync();
        }

        //public async virtual Task WriteEntityRelationsAsync(IEnumerable<EntityRelation> relations)
        //{
        //    context.UpdateRange(relations);
        //    await context.SaveChangesAsync();
        //}

        //public async virtual Task WriteEntityValuesAsync(IEnumerable<EntityValue> values)
        //{
        //    context.UpdateRange(values);
        //    await context.SaveChangesAsync();
        //}

        public async Task<List<E>> ListenNewAsync<E>(long lastId, TimeSpan maxWait, Func<E, bool> filter = null) where E : EntityBase
        {
            var results = await context.Set<E>().Where(x => x.id > lastId).Take(config.MaxRetrieve).ToListAsync();

            if(results.Count > 0)
                return results;

            //Oh there were none ready... go ahead and listen for changes.
            return null;
        }
    }
}