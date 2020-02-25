using System;
using System.Collections.Generic;

namespace entitysystem
{
    public class EntitySearchBase
    {
        public List<long> Ids = new List<long>();
        public DateTime CreateStart = new DateTime(0);
        public DateTime CreateEnd = new DateTime(0);
        //public bool CaseInsensitive = true;
    }

    public class EntitySearch:EntitySearchBase
    {
        public string TypeRegex = "";
        public string NameRegex = "";
        //public List<string> Types = new List<string>();
        //public List<string> Names = new List<string>();
    }

    public class EntityValueSearch : EntitySearchBase
    {
        public Dictionary<string, string> ValueRegex = new Dictionary<string, string>(); //string, List<string>> Values = new Dictionary<string, List<string>>();
        public List<long> EntityIds = new List<long>();
    }

    public class EntityRelationSearch : EntitySearchBase
    {
        public string TypeRegex;
        //public List<string> Types = new List<string>();
        public List<long> EntityIds1 = new List<long>();
        public List<long> EntityIds2 = new List<long>();
    }

}