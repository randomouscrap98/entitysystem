using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem.Implementations
{
    public class EntityQueryableEfCoreConfig
    {
        public int ConcurrentAccess {get;set;} = 10;
    }

    public class EntityQueryableEfCore : IEntityQueryable
    {
        public DbContext context;
        protected ILogger<EntityQueryableEfCore> logger;
        protected EntityQueryableEfCoreConfig config;

        protected SemaphoreSlim accessLimiter;

        public EntityQueryableEfCore(ILogger<EntityQueryableEfCore> logger, DbContext context, EntityQueryableEfCoreConfig config)
        {
            this.context = context;
            //this.context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            this.config = config;
            this.logger = logger;

            this.accessLimiter = new SemaphoreSlim(config.ConcurrentAccess, config.ConcurrentAccess);
        }

        public IQueryable<E> GetQueryable<E>() where E : EntityBase 
        { 
            return context.Set<E>().AsNoTracking(); 
        }

        public async Task<List<E>> GetListAsync<E>(IQueryable<E> query) 
        { 
            await accessLimiter.WaitAsync();
            try { return await query.ToListAsync();  }
            finally { accessLimiter.Release(); }
        }

        public async Task<T> GetMaxAsync<T,E>(IQueryable<E> query, Expression<Func<E, T>> selector)
        {
            await accessLimiter.WaitAsync();
            try { return await query.MaxAsync(selector);  }
            finally { accessLimiter.Release(); }
        }

        public Task<List<E>> GetAllAsync<E>() where E : EntityBase { return GetListAsync(GetQueryable<E>()); }

        protected async Task SaveChangesAsync()
        {
            await accessLimiter.WaitAsync();
            try { await context.SaveChangesAsync(); }
            finally { accessLimiter.Release(); }
        }

        public Task DeleteAsync<E>(params E[] items) where E : EntityBase
        {
            logger.LogTrace($"DeleteAsync called for {items.Count()} {typeof(E).Name} items");
            context.RemoveRange(items);
            return SaveChangesAsync();
        }

        public async Task WriteAsync<E>(params E[] items) where E : EntityBase
        {
            //Yes, we let efcore do all the work. if something weird happens... oh well. this class
            //isn't meant for safety... I think?
            logger.LogTrace($"WriteAsync called for {items.Count()} {typeof(E).Name} items");
            context.UpdateRange(items);

            await SaveChangesAsync();

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached)
                .ToList();

            foreach (var entry in entries)
                if (entry.Entity != null)
                    entry.State = EntityState.Detached;
            //Still being tracked???
            //for(var eb in items)
            //    eb.
        }
    }
}