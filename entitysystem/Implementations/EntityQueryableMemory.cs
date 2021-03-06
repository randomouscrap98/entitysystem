using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem.Implementations
{

    public class EntityQueryableMemory : IEntityQueryable
    {
        public List<EntityBase> AllItems = new List<EntityBase>();
        protected ILogger<EntityQueryableMemory> logger;

        public EntityQueryableMemory(ILogger<EntityQueryableMemory> logger)
        {
            this.logger = logger;
        }

        public Task<IQueryable<E>> GetQueryableAsync<E>() where E : EntityBase => Task.FromResult(AllItems.Where(x => x is E).Select(x => (E)x).AsQueryable());
        public Task<List<E>> GetListAsync<E>(IQueryable<E> query) => Task.FromResult(query.ToList());
        public Task<T> GetMaxAsync<T,E>(IQueryable<E> query, Expression<Func<E, T>> selector) => Task.FromResult(query.Max(selector));
        public async Task<List<E>> GetAllAsync<E>() where E : EntityBase { return await GetListAsync(await GetQueryableAsync<E>()); }

        public Task DeleteAsync<E>(params E[] items) where E : EntityBase
        {
            logger.LogTrace($"DeleteAsync called for {items.Count()} {typeof(E).Name} items");
            AllItems.RemoveAll(x => x is E && items.Any(y => y.id == x.id));
            return Task.CompletedTask;
        }

        public Task WriteAsync<E>(params E[] items) where E : EntityBase
        {
            logger.LogTrace($"WriteAsync called for {items.Count()} {typeof(E).Name} items");

            foreach(var item in items)
            {
                if(item.id == 0)
                    item.id = AllItems.Count(x => x is E) + 1;

                var existing = AllItems.FirstOrDefault(x => x is E && x.id == item.id);

                if (existing != null)
                {
                    item.createDate = existing.createDate;
                    AllItems.Remove(existing);
                }

                AllItems.Add(item);
            }

            return Task.CompletedTask;
        }

    }
}