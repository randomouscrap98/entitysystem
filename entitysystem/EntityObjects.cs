using System;
using System.Collections.Generic;
using System.Linq;

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

    public class EntityPackage
    {
        public Entity Entity {get;set;}

        //There are usually a small amount of these per item
        public Dictionary<string, List<EntityValue>> Values {get;set;} = new Dictionary<string, List<EntityValue>>();
        public Dictionary<string, List<EntityRelation>> ParentRelations {get;set;} = new Dictionary<string, List<EntityRelation>>();

        //I USUALLY don't like putting functions in data objects but I think these are fine.
        protected T BasicGrab<T>(Dictionary<string, List<T>> values, string key)
        {
            if(!values.ContainsKey(key))
                throw new InvalidOperationException($"No key {key} in {GetType().Name}");
            
            if(values[key].Count != 1)
                throw new InvalidOperationException($"Not a singular value for key {key} in {GetType().Name}");
            
            return values[key].First();
        }

        public override int GetHashCode()
        {
            return Entity.id.GetHashCode();
        }

        protected List<T> SetupForEquality<T>(Dictionary<string, List<T>> dic) where T : EntityBase
        {
            return dic.SelectMany(x => x.Value).OrderBy(x => x.id).ToList();
        }

        public bool Equals(EntityPackage package)
        {
            return Entity.Equals(package.Entity) && 
                SetupForEquality(Values).SequenceEqual(SetupForEquality(package.Values)) &&
                SetupForEquality(ParentRelations).SequenceEqual(SetupForEquality(package.ParentRelations));
        }

        public override bool Equals(object obj)
        {
            if(obj != null && obj is EntityPackage) //this.GetType().Equals(obj.GetType()))
                return Equals((EntityPackage)obj); //EqualsSelf(obj);
            else
                return false;
        }

        public EntityValue GetValue(string key) { return BasicGrab(Values, key); }
        public EntityRelation GetRelation(string type) { return BasicGrab(ParentRelations, type); }
    }
}