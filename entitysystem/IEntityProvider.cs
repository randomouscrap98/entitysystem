using System.Collections.Generic;
using System.Threading.Tasks;

namespace entitysystem 
{
    public interface IEntityProvider
    {
        Task<List<Entity>> GetEntitiesAsync(EntitySearch search);
        Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search);
        Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search);

        Task WriteEntitiesAsync(IEnumerable<Entity> entities);
        Task WriteEntityValuesAsync(IEnumerable<EntityValue> values);
        Task WriteEntityRelationsAsync(IEnumerable<EntityRelation> relations);
    }
}