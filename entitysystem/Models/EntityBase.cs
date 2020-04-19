using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomous.EntitySystem
{
    /// <summary>
    /// All items in this system have these fields
    /// </summary>
    public class EntityBase
    {
        public long id {get;set;}

        /// <summary>
        /// WARN: "Kind" is lost on sqlite databases! When converting to views, consider setting the timezone/kind manually!
        /// </summary>
        /// <value></value>
        public DateTime? createDate {get;set;}

        /// <summary>
        /// Get the createDate with "proper" kind set (assumes same system wrote the createDate as the one reading it!!)
        /// </summary>
        /// <returns></returns>
        public DateTime? createDateProper()
        {   
            if(createDate == null)
                return null;

            return new DateTime(((DateTime)createDate).Ticks, DateTime.Now.Kind);
        }

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

        public EntityBase() : this(true) {}

        public EntityBase(bool createNow) 
        {
            if(createNow)
                createDate = DateTime.Now;
        }

        public EntityBase(EntityBase copy)
        {
            id = copy.id;
            createDate = copy.createDate;
        }
    }
}