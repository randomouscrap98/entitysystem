using System;
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
        public IQueryable<T> ApplyGeneric<T>(IQueryable<T> query, EntitySearchBase search, bool finalize) where T : EntityBase
        {
            if(search.Ids.Count > 0)
                query = query.Where(x => search.Ids.Contains(x.id));

            if(search.CreateEnd.Ticks > 0)
                query = query.Where(x => x.createDate <= search.CreateEnd);
            if(search.CreateStart.Ticks > 0)
                query = query.Where(x => x.createDate >= search.CreateStart);

            if(search.MaxId >= 0)
                query = query.Where(x => x.id < search.MaxId);
            if(search.MinId >= 0)
                query = query.Where(x => x.id > search.MinId);

            if(finalize)
                query = ApplyFinal(query, search);

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
            var sort = search.Sort.ToLower();

            if(sort == "id")
            {
                if (search.Reverse)
                    query = query.OrderByDescending(x => x.id);
                else
                    query = query.OrderBy(x => x.id);
            }
            else if(sort == "random")
            {
                long modulo = 131071; //8191;
                long random = (new Random()).Next() & modulo;
                //query = query.OrderBy(x => ((x.id + random) * 7459) & modulo);
                query = query.OrderBy(x => ((x.id + random) * 66284) & modulo);
            }

            if(search.Skip >= 0)
                query = query.Skip(search.Skip);
            if(search.Limit >= 0)
                query = query.Take(search.Limit);

            return query;
        }

        public IQueryable<Entity> ApplyEntitySearch(IQueryable<Entity> query, EntitySearch search, bool finalize)
        {
            query = ApplyGeneric(query, search, false);
            
            if(!string.IsNullOrEmpty(search.NameLike))
                query = query.Where(x => EF.Functions.Like(x.name, search.NameLike));

            if(!string.IsNullOrEmpty(search.TypeLike))
                query = query.Where(x => EF.Functions.Like(x.type, search.TypeLike));

            if(finalize)
                query = ApplyFinal(query, search);
            
            return query;
        }

        public IQueryable<EntityValue> ApplyEntityValueSearch(IQueryable<EntityValue> query, EntityValueSearch search, bool finalize)
        {
            query = ApplyGeneric(query, search, false);

            if(!string.IsNullOrEmpty(search.KeyLike))
                query = query.Where(x => EF.Functions.Like(x.key, search.KeyLike));

            if(!string.IsNullOrEmpty(search.ValueLike))
                query = query.Where(x => EF.Functions.Like(x.value, search.ValueLike));

            if(search.EntityIds.Count > 0)
                query = query.Where(x => search.EntityIds.Contains(x.entityId));

            if(finalize)
                query = ApplyFinal(query, search);

            return query;
        }

        public IQueryable<EntityRelation> ApplyEntityRelationSearch(IQueryable<EntityRelation> query, EntityRelationSearch search, bool finalize)
        {
            query = ApplyGeneric<EntityRelation>(query, search, false);

            if(!string.IsNullOrEmpty(search.TypeLike))
                query = query.Where(x => EF.Functions.Like(x.type, search.TypeLike));

            if(search.EntityIds1.Count > 0)
                query = query.Where(x => search.EntityIds1.Contains(x.entityId1));

            if(search.EntityIds2.Count > 0)
                query = query.Where(x => search.EntityIds2.Contains(x.entityId2));

            if(finalize)
                query = ApplyFinal(query, search);

            return query;
        }
    }
}