using System.Collections.Generic;
using System.Linq;
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

        public IQueryable<E> GetQueryable<E>() where E : EntityBase => AllItems.Where(x => x is E).Select(x => (E)x).AsQueryable();
        public Task<List<E>> GetList<E>(IQueryable<E> query) => Task.FromResult(query.ToList());
        public async Task<List<E>> GetAll<E>() where E : EntityBase { return await GetList(GetQueryable<E>()); }

        public Task DeleteAsync<E>(params E[] items) where E : EntityBase
        {
            logger.LogTrace($"DeleteAsync called for {items.Count()} {typeof(E).Name} items");
            AllItems.RemoveAll(x => x is E && items.Any(y => y.id == x.id));
            //FinalizeWrite(items);
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

            //FinalizeWrite(items);
            return Task.CompletedTask;
        }

    }
}