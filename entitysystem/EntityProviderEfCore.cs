using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace entitysystem
{
    public class EntityProviderEfCoreConfig
    {
        public int MaxRetrieve {get;set;} = 10000;
    }

    public class EntityListener : IDisposable // <E> where E : EntityBase
    {
        public EntityBase SignaledEntity = null;
        public Func<EntityBase, bool> Filter;
        public AutoResetEvent Signal = new AutoResetEvent(false);

        //I don't CARE if this is "not the right way to do dispose", the right way is GARBAGE and I am
        //the only one calling dispose so... I think I'll be fine.
        public void Dispose()
        {
            Signal.Dispose();
        }
    }

    public class EntityProviderEfCore : IEntityProvider
    {
        protected ILogger logger;
        protected IEntitySearcher searcher;
        public DbContext context;
        public EntityProviderEfCoreConfig config;

        private List<EntityListener> listeners = new List<EntityListener>();
        private readonly object listenLock = new object();

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
            logger.LogTrace($"WriteAsync called for {items.Count()} {typeof(E).Name} items");
            context.UpdateRange(items);
            await context.SaveChangesAsync();

            //Assuming saving actually works, let's go alert everyone
            lock(listenLock)
            {
                foreach(var item in items)
                {
                    try
                    {
                        var signalers = listeners.Where(x => x.Filter(item));

                        foreach (var signal in signalers)
                            signal.Signal.Set();
                    }
                    catch(Exception ex)
                    {
                        logger.LogError($"Error while signalling for item {item.id}: {ex}");
                    }
                }
            }
        }

        public async Task<bool> ListenAsync(EntityListener listener, TimeSpan maxWait)
        {
            bool signaled = false;
            await Task.Run(() => signaled = listener.Signal.WaitOne(maxWait));
            return signaled;
        }

        public async Task<List<E>> ListenNewAsync<E>(long lastId, TimeSpan maxWait, Func<E, bool> filter = null) where E : EntityBase
        {
            logger.LogTrace($"ListenNewAsync called for lastId {lastId}, maxWait {maxWait}");
            filter = filter ?? new Func<E, bool>((x) => true);

            var results = await context.Set<E>().Where(x => x.id > lastId).Take(config.MaxRetrieve).ToListAsync();
            results = results.Where(x => filter(x)).ToList(); //Maybe find a more elegant way to do this?

            if(results.Count > 0)
                return results;

            using(var listener = new EntityListener())
            {
                listener.Filter = (e) => e is E && e.id > lastId && filter((E)e);

                lock(listenLock)
                    listeners.Add(listener);

                try
                {
                    if (await ListenAsync(listener, maxWait))
                        return new List<E>() { (E)listener.SignaledEntity };
                }
                finally
                {
                    //WE have to remove it because we added it
                    lock(listenLock)
                        listeners.Remove(listener);
                }
            }

            //Oh there were none ready... go ahead and listen for changes.
            return null;
        }
    }
}