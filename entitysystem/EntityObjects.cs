using System;

namespace entitysystem
{
    /// <summary>
    /// All items in this system have these fields
    /// </summary>
    public class EntityBase
    {
        public long id {get;set;}
        public DateTime createDate {get;set;}
    }

    /// <summary>
    /// A simple unit of "something" in a data storage system
    /// </summary>
    public class Entity : EntityBase
    {
        public string name {get;set;}
        public string content {get;set;}
        public string type {get;set;}
    }

    /// <summary>
    /// A simple relationship between two "somethings" in a data storage system
    /// </summary>
    public class EntityRelation : EntityBase
    {
        public long entityId1 {get;set;}
        public long entityId2 {get;set;}
        public string type {get;set;}
        public string value {get;set;}
    }

    /// <summary>
    /// Extra data for a "something" (key value pairs)
    /// </summary>
    public class EntityValue : EntityBase
    {
        public long entityId {get;set;}
        public string key {get;set;}
        public string value{get;set;}
    }
}