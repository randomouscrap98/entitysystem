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
        public List<long> Ids {get;set;} = new List<long>();
        public DateTime CreateStart {get;set;} = new DateTime(0);
        public DateTime CreateEnd {get;set;} = new DateTime(0);

        /// <summary>
        /// Only ids strictly lower 
        /// </summary>
        /// <value></value>
        public long MaxId {get;set;} = -1;
        /// <summary>
        /// Only ids strictly higher
        /// </summary>
        /// <value></value>
        public long MinId {get;set;} = -1;
        public int Limit {get;set;} = -1;
        public int Skip {get;set;} = -1;
        public bool Reverse {get;set;} = false;
    }

    /// <summary>
    /// Search parameters for entities
    /// </summary>
    public class EntitySearch:EntitySearchBase
    {
        public string TypeLike {get;set;} = "";
        public string NameLike {get;set;} = "";
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
        public string KeyLike {get;set;} = "";
        public string ValueLike {get;set;} = "";
        public List<long> EntityIds {get;set;} = new List<long>();
    }

    /// <summary>
    /// Search parameters for entity relationships
    /// </summary>
    public class EntityRelationSearch : EntitySearchBase
    {
        public string TypeLike {get;set;} = "";
        public List<long> EntityIds1 {get;set;} = new List<long>();
        public List<long> EntityIds2 {get;set;} = new List<long>();
    }

}