using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Randomous.EntitySystem 
{
    public interface IEntityProvider
    {
        Task<List<Entity>> GetEntitiesAsync(EntitySearch search);
        Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search);
        Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search);

        Task WriteAsync<E>(IEnumerable<E> entities) where E : EntityBase;
        Task DeleteAsync<E>(IEnumerable<E> items) where E : EntityBase;
        Task<List<E>> ListenNewAsync<E>(long lastId, TimeSpan maxWait, Func<E, bool> filter = null) where E : EntityBase;
    }
}