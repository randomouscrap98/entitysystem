using System;
using System.Collections.Generic;

// Why have separate entity search objects instead of querying directly? The data might not be stored the way you think!
// What if one day, IQueryable isn't valid anymore? Objects will always be valid, and lists probably too
namespace Randomous.EntitySystem
{
    /// <summary>
    /// Base search for ALL kinds of entity search
    /// </summary>
    public class EntitySearchBase
    {
        public List<long> Ids = new List<long>();
        public DateTime CreateStart = new DateTime(0);
        public DateTime CreateEnd = new DateTime(0);
    }

    /// <summary>
    /// Search parameters for entities
    /// </summary>
    public class EntitySearch:EntitySearchBase
    {
        public string TypeLike = "";
        public string NameLike = "";
    }

    /// <summary>
    /// Search parameters for entity values
    /// </summary>
    /// <remarks>
    /// Each search applies to JUST the table it applies to. So, even though there may be many keys and values per 
    /// content, you should only be searching for ONE of each per search. You can merge them later; don't overcomplicate
    /// the system.
    /// </remarks>
    public class EntityValueSearch : EntitySearchBase
    {
        public string KeyLike = "";
        public string ValueLike = "";
        public List<long> EntityIds = new List<long>();
    }

    /// <summary>
    /// Search parameters for entity relationships
    /// </summary>
    public class EntityRelationSearch : EntitySearchBase
    {
        public string TypeLike;
        //public List<string> Types = new List<string>();
        public List<long> EntityIds1 = new List<long>();
        public List<long> EntityIds2 = new List<long>();
    }

}