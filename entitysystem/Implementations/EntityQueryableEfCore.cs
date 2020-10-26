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
        public TimeSpan MaxLockWait {get;set;} = TimeSpan.FromSeconds(3);
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

        public async Task<T> LockAsync<T>(Func<Task<T>> func)
        {
            if(!await accessLimiter.WaitAsync(config.MaxLockWait))
                throw new TimeoutException($"Couldn't grab Query lock in time! {config.MaxLockWait}");
            try { return await func(); }
            finally { accessLimiter.Release(); }
        }

        public Task LockAsync(Func<Task> func)
        {
            return LockAsync<bool>(async () => 
            {
                await func();
                return true;
            });
        }

        public T Lock<T>(Func<T> func)
        {
            return LockAsync(() => Task.FromResult(func())).Result;
        }

        public Task<IQueryable<E>> GetQueryableAsync<E>() where E : EntityBase 
        { 
            return LockAsync(() => Task.FromResult(context.Set<E>().AsNoTracking()));
        }

        public Task<List<E>> GetListAsync<E>(IQueryable<E> query) 
        { 
            return LockAsync(() => query.ToListAsync());
        }

        public Task<T> GetMaxAsync<T,E>(IQueryable<E> query, Expression<Func<E, T>> selector)
        {
            return LockAsync(() => query.MaxAsync(selector));
        }

        public async Task<List<E>> GetAllAsync<E>() where E : EntityBase 
        { 
            var queryable = await GetQueryableAsync<E>();
            return await GetListAsync(queryable);
        }

        protected Task SaveChangesAsync()
        {
            return LockAsync(() => context.SaveChangesAsync());
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