using System;

namespace Randomous.EntitySystem
{
    /// <summary>
    /// All items in this system have these fields
    /// </summary>
    public class EntityBase
    {
        public long id {get;set;}
        public DateTime createDate {get;set;}

        protected virtual bool EqualsSelf(object obj)
        {
            var other = (EntityBase)obj;
            return other.id == id && other.createDate == createDate;
        }

        public override bool Equals(object obj)
        {
            if(obj != null && this.GetType().Equals(obj.GetType()))
                return EqualsSelf(obj);
            else
                return false;
        }

        public override int GetHashCode() 
        { 
            return id.GetHashCode(); 
        }
    }

    /// <summary>
    /// A simple unit of "something" in a data storage system
    /// </summary>
    public class Entity : EntityBase
    {
        public string name {get;set;}
        public string content {get;set;}
        public string type {get;set;}

        protected override bool EqualsSelf(object obj)
        {
            var other = (Entity)obj;
            return base.EqualsSelf(obj) && name == other.name && content == other.content && type == other.type;
        }
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

        protected override bool EqualsSelf(object obj)
        {
            var other = (EntityRelation)obj;
            return base.EqualsSelf(other) && entityId1 == other.entityId1 && entityId2 == other.entityId2 && 
                    type == other.type && value == other.value;
        }
    }

    /// <summary>
    /// Extra data for a "something" (key value pairs)
    /// </summary>
    public class EntityValue : EntityBase
    {
        public long entityId {get;set;}
        public string key {get;set;}
        public string value{get;set;}

        protected override bool EqualsSelf(object obj)
        {
            var other = (EntityValue)obj;
            return base.EqualsSelf(other) && entityId == other.entityId && key == other.key && value == other.value;
        }
    }
}