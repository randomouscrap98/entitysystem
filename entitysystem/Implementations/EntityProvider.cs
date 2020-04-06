using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem.Implementations
{
    public class EntityProvider : IEntityProvider
    {
        public IEntityQueryable Query {get;}
        public IEntitySearcher Search {get;}
        public IEntityExpander Expand {get;}

        protected ILogger<EntityProvider> logger;
        protected ISignaler<EntityBase> signaler;

        public EntityProvider(ILogger<EntityProvider> logger, IEntityQueryable query,
            IEntitySearcher searcher, IEntityExpander expander, ISignaler<EntityBase> signaler)
        {
            this.logger = logger;
            this.Query = query;
            this.Search = searcher;
            this.Expand = expander;
            this.signaler = signaler;
        }

        public async Task<List<E>> ListenNewAsync<E>(long lastId, TimeSpan maxWait, Func<E, bool> filter = null) where E : EntityBase
        {
            logger.LogTrace($"ListenNewAsync called for lastId {lastId}, maxWait {maxWait}");
            filter = filter ?? new Func<E, bool>((x) => true);

            var results = await Query.GetList(Query.GetQueryable<E>().Where(x => x.id > lastId).Select(x => (E)x));
            results = results.Where(x => filter(x)).ToList(); //Maybe find a more elegant way to do this?

            if(results.Count > 0)
                return results;
            else
                return (await signaler.ListenAsync((e) => e is E && e.id > lastId && filter((E)e), maxWait)).Cast<E>().ToList();
        }
    }
}