using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem
{
    //public class EntityProviderEfCoreConfig
    //{
    //    public int MaxRetrieve {get;set;} = 10000;
    //}

    public class EntityProviderEfCore : EntityProviderBase, IEntityProvider
    {
        public DbContext context;
        //public EntityProviderEfCoreConfig config;

        public EntityProviderEfCore(ILogger<EntityProviderEfCore> logger, IEntitySearcher searcher, 
            DbContext context, ISignaler<EntityBase> signaler) //, EntityProviderEfCoreConfig config)
        {
            this.searcher = searcher;
            this.logger = logger;
            this.context = context;
            //this.config = config;
            this.signaler = signaler;
        }

        protected override IQueryable<E> GetQueryable<E>() { return context.Set<E>(); }
        protected override async Task<List<E>> GetList<E>(IQueryable<E> query) { return await query.ToListAsync(); }

        public async Task DeleteAsync<E>(IEnumerable<E> items) where E : EntityBase
        {
            logger.LogTrace($"DeleteAsync called for {items.Count()} {typeof(E).Name} items");
            context.RemoveRange(items);
            await context.SaveChangesAsync();
            FinalizeWrite(items);
        }

        public async virtual Task WriteAsync<E>(IEnumerable<E> items) where E : EntityBase
        {
            //Yes, we let efcore do all the work. if something weird happens... oh well. this class
            //isn't meant for safety... I think?
            logger.LogTrace($"WriteAsync called for {items.Count()} {typeof(E).Name} items");
            context.UpdateRange(items);
            await context.SaveChangesAsync();
            FinalizeWrite(items);
        }
    }
}