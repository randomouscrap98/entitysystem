using System;
using System.Collections.Generic;
using System.Linq;
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

        Task<List<E>> ListenAsync<E>(object listenId, Func<IQueryable<E>, IQueryable<E>> filter, TimeSpan maxWait) where E : EntityBase;
        List<ListenerData> Listeners {get;}
    }
}