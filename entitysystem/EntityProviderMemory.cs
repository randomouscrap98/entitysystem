using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace entitysystem
{
    public class EntityProviderMemory : IEntityProvider
    {
        protected ILogger logger;
        protected IEntitySearcher searcher;

        public List<Entity> Entities = new List<Entity>();
        public List<EntityValue> Values = new List<EntityValue>();
        public List<EntityRelation> Relations = new List<EntityRelation>();

        public EntityProviderMemory(ILogger<EntityProviderMemory> logger, IEntitySearcher searcher)
        {
            this.searcher = searcher;
            this.logger = logger;
        }

        public Task<List<Entity>> GetEntitiesAsync(EntitySearch search)
        {
            logger.LogTrace("GetEntitiesAsync called");
            return Task.FromResult(searcher.ApplyEntitySearch(Entities.AsQueryable(), search).ToList());
        }

        public Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search)
        {
            throw new System.NotImplementedException();
        }

        public Task WriteEntities(IEnumerable<Entity> entities)
        {
            throw new System.NotImplementedException();
        }

        public Task WriteEntityRelations(IEnumerable<EntityRelation> relations)
        {
            throw new System.NotImplementedException();
        }

        public Task WriteEntityValues(IEnumerable<EntityValue> values)
        {
            throw new System.NotImplementedException();
        }
    }
}