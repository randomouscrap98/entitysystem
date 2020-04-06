using System;
using System.Collections.Generic;
using System.Linq;

namespace Randomous.EntitySystem
{
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
            if(obj != null && obj is EntityPackage)
                return Equals((EntityPackage)obj);
            else
                return false;
        }

        public EntityValue GetValue(string key) { return BasicGrab(Values, key); }
        public EntityRelation GetRelation(string type) { return BasicGrab(ParentRelations, type); }
    }
}