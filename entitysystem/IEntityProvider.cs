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

        Task WriteAsync<E>(params E[] entities) where E : EntityBase;
        Task WriteAsync(params EntityPackage[] entities);
        Task DeleteAsync<E>(params E[] items) where E : EntityBase;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Notice "lastId". You might think this could be generic and ONLY have a filter. No,
        /// the "lastId" is an optimization so the ENTIRE dang table doesn't have to be searched, because
        /// EFCore is weird. Keep lastId, this is our standard use case anyway. Filter is just extra.
        /// </remarks>
        /// <param name="lastId"></param>
        /// <param name="maxWait"></param>
        /// <param name="filter"></param>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        Task<List<E>> ListenNewAsync<E>(long lastId, TimeSpan maxWait, Func<E, bool> filter = null) where E : EntityBase;

        void AddValues(EntityPackage package, params EntityValue[] values);
        void AddRelations(EntityPackage package, params EntityRelation[] relations);

        Task<List<EntityPackage>> ExpandAsync(params Entity[] entities);
    }
}