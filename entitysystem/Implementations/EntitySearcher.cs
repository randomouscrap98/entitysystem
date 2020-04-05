using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem.Implementations
{
    /// <summary>
    /// A simple queryable reducer for various entity searches. Narrows a query
    /// based on the given search object
    /// </summary>
    public class EntitySearcher : IEntitySearcher
    {
        public ILogger logger;

        public EntitySearcher(ILogger<EntitySearcher> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Apply generic base search parameters such as ids and create date constraints. 
        /// </summary>
        /// <remarks>
        /// All interface search functions should call this
        /// </remarks>
        /// <param name="query"></param>
        /// <param name="search"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> ApplyGeneric<T>(IQueryable<T> query, EntitySearchBase search) where T : EntityBase
        {
            if(search.Ids.Count > 0)
                query = query.Where(x => search.Ids.Contains(x.id));

            if(search.CreateEnd.Ticks > 0)
                query = query.Where(x => x.createDate <= search.CreateEnd);
            if(search.CreateStart.Ticks > 0)
                query = query.Where(x => x.createDate >= search.CreateStart);

            return query;
        }

        /// <summary>
        /// Apply generic final search parameters such as limit/skip/reverse
        /// </summary>
        /// <param name="query"></param>
        /// <param name="search"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> ApplyFinal<T>(IQueryable<T> query, EntitySearchBase search) where T : EntityBase
        {
            if(search.Reverse)
                query = query.OrderByDescending(x => x.id);
            else
                query = query.OrderBy(x => x.id);

            if(search.Skip >= 0)
                query = query.Skip(search.Skip);
            if(search.Limit >= 0)
                query = query.Take(search.Limit);

            return query;
        }

        public IQueryable<Entity> ApplyEntitySearch(IQueryable<Entity> query, EntitySearch search)
        {
            query = ApplyGeneric(query, search);
            
            if(!string.IsNullOrEmpty(search.NameLike))
                query = query.Where(x => EF.Functions.Like(x.name, search.NameLike));

            if(!string.IsNullOrEmpty(search.TypeLike))
                query = query.Where(x => EF.Functions.Like(x.type, search.TypeLike));

            return ApplyFinal(query, search);
        }

        public IQueryable<EntityValue> ApplyEntityValueSearch(IQueryable<EntityValue> query, EntityValueSearch search)
        {
            query = ApplyGeneric(query, search);

            if(!string.IsNullOrEmpty(search.KeyLike))
                query = query.Where(x => EF.Functions.Like(x.key, search.KeyLike));

            if(!string.IsNullOrEmpty(search.ValueLike))
                query = query.Where(x => EF.Functions.Like(x.value, search.ValueLike));

            if(search.EntityIds.Count > 0)
                query = query.Where(x => search.EntityIds.Contains(x.entityId));

            return ApplyFinal(query, search);
        }

        public IQueryable<EntityRelation> ApplyEntityRelationSearch(IQueryable<EntityRelation> query, EntityRelationSearch search)
        {
            query = ApplyGeneric<EntityRelation>(query, search);

            if(!string.IsNullOrEmpty(search.TypeLike))
                query = query.Where(x => EF.Functions.Like(x.type, search.TypeLike));

            if(search.EntityIds1.Count > 0)
                query = query.Where(x => search.EntityIds1.Contains(x.entityId1));

            if(search.EntityIds2.Count > 0)
                query = query.Where(x => search.EntityIds2.Contains(x.entityId2));

            return ApplyFinal(query, search);
        }
    }
}