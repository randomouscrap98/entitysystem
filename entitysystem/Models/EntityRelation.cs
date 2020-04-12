namespace Randomous.EntitySystem
{
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

        public EntityRelation() : base() {}
        public EntityRelation(bool createNow) : base(createNow) {}

        public EntityRelation(EntityRelation copy) : base(copy)
        {
            entityId1 = copy.entityId1;
            entityId2 = copy.entityId2;
            type = copy.type;
            value = copy.value;
        }
    }
}