using System.Collections.Generic;
using System.Linq;

namespace Randomous.EntitySystem
{
    public class EntityPackage
    {
        public Entity Entity {get;set;}
        public List<EntityValue> Values {get;set;} = new List<EntityValue>();

        /// <summary>
        /// Relations where this entity is entity2 (the child). These are all parents
        /// </summary>
        /// <value></value>
        public List<EntityRelation> Relations {get;set;} = new List<EntityRelation>();

        public EntityPackage() {}
        public EntityPackage(EntityPackage copy)
        {
            Entity = new Entity(copy.Entity);
            Values = new List<EntityValue>(copy.Values.Select(x => new EntityValue(x)));
            Relations = new List<EntityRelation>(copy.Relations.Select(x => new EntityRelation(x)));
        }

        public override bool Equals(object obj)
        {
            if(obj != null && obj is EntityPackage)
            {
                var other = (EntityPackage)obj;
                return Entity.Equals(other.Entity) && Values.SequenceEqual(other.Values)
                    && Relations.SequenceEqual(other.Relations);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() 
        { 
            return Entity.GetHashCode(); 
        }
    }
}