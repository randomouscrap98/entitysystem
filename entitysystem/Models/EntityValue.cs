namespace Randomous.EntitySystem
{
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