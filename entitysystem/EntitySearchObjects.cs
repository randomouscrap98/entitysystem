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
        public int Limit = -1;
        public int Skip = -1;
        public bool Reverse = false;

        public EntitySearchBase() {}
        public EntitySearchBase(EntitySearchBase copy)
        {
            Ids = new List<long>(copy.Ids);
            CreateStart = copy.CreateStart;
            CreateEnd = copy.CreateEnd;
            Limit = copy.Limit;
            Skip = copy.Skip;
            Reverse = copy.Reverse;
        }
    }

    /// <summary>
    /// Search parameters for entities
    /// </summary>
    public class EntitySearch:EntitySearchBase
    {
        public string TypeLike = "";
        public string NameLike = "";

        public EntitySearch() {}
        public EntitySearch(EntitySearchBase copy) : base(copy) {}
        public EntitySearch(EntitySearch copy) : base(copy)
        {
            TypeLike = copy.TypeLike;
            NameLike = copy.NameLike;
        }
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

        public EntityValueSearch() {}
        public EntityValueSearch(EntitySearchBase copy) : base(copy) {}
        public EntityValueSearch(EntityValueSearch copy) : base(copy)
        {
            KeyLike = copy.KeyLike;
            ValueLike = copy.ValueLike;
            EntityIds = new List<long>(copy.EntityIds);
        }
    }

    /// <summary>
    /// Search parameters for entity relationships
    /// </summary>
    public class EntityRelationSearch : EntitySearchBase
    {
        public string TypeLike;
        public List<long> EntityIds1 = new List<long>();
        public List<long> EntityIds2 = new List<long>();

        public EntityRelationSearch() {}
        public EntityRelationSearch(EntitySearchBase copy) : base(copy) {}
        public EntityRelationSearch(EntityRelationSearch copy) : base(copy)
        {
            TypeLike = copy.TypeLike;
            EntityIds1 = new List<long>(copy.EntityIds1);
            EntityIds2 = new List<long>(copy.EntityIds2);
        }
    }

}