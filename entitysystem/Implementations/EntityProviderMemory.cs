using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem.Implementations
{
    public class EntityProviderMemory : EntityProviderBase, IEntityQueryable
    {
        public EntityProviderMemory(ILoggerFactory logFactory) 
        {
            //No dependency injection here: nobody should know what we're doing because we're some magical in-memory tester class.
            //Just assume it's a black box that works!
            this.services = new EntityProviderBaseServices(
                logFactory.CreateLogger<EntityProviderMemory>(),
                new EntitySearcher(logFactory.CreateLogger<EntitySearcher>()),
                new SignalSystem<EntityBase>(logFactory.CreateLogger<SignalSystem<EntityBase>>()),
                new GeneralHelper()
            );
        }

        public List<EntityBase> AllItems = new List<EntityBase>();

        public override IQueryable<E> GetQueryable<E>() => AllItems.Where(x => x is E).Select(x => (E)x).AsQueryable();
        public override Task<List<E>> GetList<E>(IQueryable<E> query) => Task.FromResult(query.ToList());

        public Task DeleteAsync<E>(params E[] items) where E : EntityBase
        {
            services.Logger.LogTrace($"DeleteAsync called for {items.Count()} {typeof(E).Name} items");
            AllItems.RemoveAll(x => x is E && items.Any(y => y.id == x.id));
            FinalizeWrite(items);
            return Task.CompletedTask;
        }

        public override Task WriteAsync<E>(params E[] items) 
        {
            services.Logger.LogTrace($"WriteAsync called for {items.Count()} {typeof(E).Name} items");

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