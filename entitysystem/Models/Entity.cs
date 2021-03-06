namespace Randomous.EntitySystem
{
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

        public Entity() {}
        public Entity(bool createNow) : base(createNow) {}

        public Entity(Entity copy) : base(copy)
        {
            name = copy.name;
            content = copy.content;
            type = copy.type;
        }
    }
}
