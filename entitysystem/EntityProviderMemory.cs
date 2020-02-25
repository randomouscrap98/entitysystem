using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace entitysystem
{
    public class EntityProviderMemory : IEntityProvider
    {
        protected ILogger logger;
        protected EntitySearchHelper helper;

        public EntityProviderMemory(ILogger<EntityProviderMemory> logger, EntitySearchHelper helper)
        {
            this.helper = helper;
            this.logger = logger;
            logger.LogInformation("HEY IT'S WORKING???");
        }

        public Task<List<Entity>> GetEntitiesAsync(EntitySearch search)
        {
            throw new System.NotImplementedException();
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