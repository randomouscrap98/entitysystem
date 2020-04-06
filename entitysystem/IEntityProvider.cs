using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Randomous.EntitySystem
{
    //A provider is everything wrapped up together
    public interface IEntityProvider : IEntityQueryable, IEntitySearcher //, IEntityExpander
    {
        /// <summary>
        /// A shortcut for simple retrieval of entities
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        Task<List<Entity>> GetEntitiesAsync(EntitySearch search);

        /// <summary>
        /// A shortcut for simple retrieval of relations
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search);

        /// <summary>
        /// A shortcut for simple retrieval of values
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search);

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
    }
}