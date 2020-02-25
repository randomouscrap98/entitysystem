using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace entitysystem
{
    /// <summary>
    /// A simple queryable reducer for various entity searches. Narrows a query
    /// based on the given search object
    /// </summary>
    public class EntitySearcher : IEntitySearcher
    {
        public ILogger logger;

        public EntitySearcher(ILogger<EntitySearch> logger)
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

        public IQueryable<Entity> ApplyEntitySearch(IQueryable<Entity> query, EntitySearch search)
        {
            query = ApplyGeneric<Entity>(query, search);
            
            //TODO/WARN: REGEX might not work! Check performance: EFCore is supposed to support this, AND sqlite supports it!

            if(!string.IsNullOrEmpty(search.NameRegex))
            {
                var regex = new Regex(search.NameRegex);
                query = query.Where(x => x.name != null && regex.IsMatch(x.name)); //the stupid... ugh ORM makes me have to repeat this code. I think...
            }

            if(!string.IsNullOrEmpty(search.TypeRegex))
            {
                var regex = new Regex(search.TypeRegex);
                query = query.Where(x => x.type != null && regex.IsMatch(x.type));
            }

            return query;
        }

        public IQueryable<EntityValue> ApplyEntityValueSearch(IQueryable<EntityValue> query, EntityValueSearch search)
        {
            query = ApplyGeneric<EntityValue>(query, search);

            if(!string.IsNullOrEmpty(search.KeyRegex))
            {
                var regex = new Regex(search.KeyRegex);
                query = query.Where(x => x.key != null && regex.IsMatch(x.key));
            }

            if(!string.IsNullOrEmpty(search.ValueRegex))
            {
                var regex = new Regex(search.ValueRegex);
                query = query.Where(x => x.value != null && regex.IsMatch(x.value));
            }

            if(search.EntityIds.Count > 0)
                query = query.Where(x => search.EntityIds.Contains(x.entityId));

            return query;
        }

        public IQueryable<EntityRelation> ApplyEntityRelationSearch(IQueryable<EntityRelation> query, EntityRelationSearch search)
        {
            query = ApplyGeneric<EntityRelation>(query, search);

            if(!string.IsNullOrEmpty(search.TypeRegex))
            {
                var regex = new Regex(search.TypeRegex);
                query = query.Where(x => x.type != null && regex.IsMatch(x.type));
            }

            if(search.EntityIds1.Count > 0)
                query = query.Where(x => search.EntityIds1.Contains(x.entityId1));

            if(search.EntityIds2.Count > 0)
                query = query.Where(x => search.EntityIds1.Contains(x.entityId2));

            return query;
        }
    }
}