using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace entitysystem
{
    //TODO: DON'T NAME IT THIS
    public class EntitySearcher : IEntitySearcher
    {
        public ILogger logger;

        public EntitySearcher(ILogger<EntitySearch> logger)
        {
            this.logger = logger;
        }

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
                query = query.Where(x => regex.IsMatch(x.name));
            }

            if(!string.IsNullOrEmpty(search.TypeRegex))
            {
                var regex = new Regex(search.TypeRegex);
                query = query.Where(x => regex.IsMatch(x.type));
            }

            return query;
        }

        public IQueryable<EntityValue> ApplyEntityValueSearch(IQueryable<EntityValue> query, EntityValueSearch search)
        {
            throw new System.NotImplementedException();
        }

        public IQueryable<EntityRelation> ApplyEntityRelationSearch(IQueryable<EntityRelation> query, EntityRelationSearch search)
        {
            throw new System.NotImplementedException();
        }
    }
}