using System.Collections.Generic;
using System.Threading.Tasks;

namespace entitysystem 
{
    public interface IEntityProvider
    {
        Task<List<Entity>> GetEntitiesAsync(EntitySearch search);
        Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search);
        Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search);

        Task WriteEntities(IEnumerable<Entity> entities);
        Task WriteEntityValues(IEnumerable<EntityValue> values);
        Task WriteEntityRelations(IEnumerable<EntityRelation> relations);
    }
}