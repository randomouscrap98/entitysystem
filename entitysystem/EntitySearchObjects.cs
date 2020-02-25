using System;
using System.Collections.Generic;

namespace entitysystem
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
        public string TypeRegex = "";
        public string NameRegex = "";
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
        public string KeyRegex = "";
        public string ValueRegex = "";
        public List<long> EntityIds = new List<long>();
    }

    /// <summary>
    /// Search parameters for entity relationships
    /// </summary>
    public class EntityRelationSearch : EntitySearchBase
    {
        public string TypeRegex;
        //public List<string> Types = new List<string>();
        public List<long> EntityIds1 = new List<long>();
        public List<long> EntityIds2 = new List<long>();
    }

}