using System.Linq;
using System.Text.RegularExpressions;

namespace entitysystem
{
    //TODO: DON'T NAME IT THIS
    public class EntitySearchHelper
    {
        void ApplyEntitySearch(IQueryable<Entity> query, EntitySearch search)
        {
            //TODO/WARN: REGEX might not work! Check performance: EFCore is supposed to support this, AND sqlite supports it!
            if(search.Ids.Count > 0)
                query = query.Where(x => search.Ids.Contains(x.id));

            if(!string.IsNullOrEmpty(search.NameRegex))
            {
                var regex = new Regex(search.NameRegex);
                query = query.Where(x => regex.IsMatch(x.name));
            }
        }
    }
}