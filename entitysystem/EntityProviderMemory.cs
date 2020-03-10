using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem
{
    public class EntityProviderMemory : EntityProviderBase, IEntityProvider
    {
        public EntityProviderMemory(ILoggerFactory logFactory) 
        {
            this.searcher = new EntitySearcher(logFactory.CreateLogger<EntitySearcher>());
            this.logger = logFactory.CreateLogger<EntityProviderMemory>();
            this.signaler = new SignalSystem<EntityBase>(logFactory.CreateLogger<SignalSystem<EntityBase>>());
        }

        public List<EntityBase> AllItems = new List<EntityBase>();

        protected override IQueryable<E> GetQueryable<E>() => AllItems.Where(x => x is E).Select(x => (E)x).AsQueryable();
        protected override Task<List<E>> GetList<E>(IQueryable<E> query) => Task.FromResult(query.ToList());

        public Task DeleteAsync<E>(IEnumerable<E> items) where E : EntityBase
        {
            logger.LogTrace($"DeleteAsync called for {items.Count()} {typeof(E).Name} items");
            AllItems.RemoveAll(x => x is E && items.Any(y => y.id == x.id));
            FinalizeWrite(items);
            return Task.CompletedTask;
        }

        public virtual Task WriteAsync<E>(IEnumerable<E> items) where E : EntityBase
        {
            //Yes, we let efcore do all the work. if something weird happens... oh well. this class
            //isn't meant for safety... I think?
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

            FinalizeWrite(items);
            return Task.CompletedTask;
        }
    }
}