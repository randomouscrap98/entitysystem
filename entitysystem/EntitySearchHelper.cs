using System.Linq;
using System.Text.RegularExpressions;

namespace entitysystem
{
    //TODO: DON'T NAME IT THIS
    public class EntitySearchHelper
    {
        IQueryable<T> ApplyGeneric<T>(IQueryable<T> query, EntitySearchBase search) where T : EntityBase
        {
            if(search.Ids.Count > 0)
                query = query.Where(x => search.Ids.Contains(x.id));

            if(search.CreateEnd.Ticks > 0)
                query = query.Where(x => x.createDate <= search.CreateEnd);
            if(search.CreateStart.Ticks > 0)
                query = query.Where(x => x.createDate >= search.CreateStart);

            return query;
        }

        IQueryable<Entity> ApplyEntitySearch(IQueryable<Entity> query, EntitySearch search)
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
    }
}