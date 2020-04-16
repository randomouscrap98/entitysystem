using System;
using System.Collections.Generic;
using System.Linq;

namespace Randomous.EntitySystem.Extensions
{
    public static class EntityPackageExtensions
    {
        public static EntityPackage Add(this EntityPackage entity, EntityValue value)
        {
            entity.Values.Add(value);
            return entity;
        }

        public static EntityPackage Add(this EntityPackage entity, EntityRelation relation)
        {
            entity.Relations.Add(relation);
            return entity;
        }

        private static IEnumerable<EntityValue> FindValues(EntityPackage entity, string key)
        {
            return entity.Values.Where(x => x.key == key);
        }

        private static IEnumerable<EntityRelation> FindRelations(EntityPackage entity, string type)
        {
            return entity.Relations.Where(x => x.type == type);
        }

        /// <summary>
        /// Get a value from an entity package
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static EntityValue GetValue(this EntityPackage entity, string key)
        {
            var values = FindValues(entity, key);

            if(values.Count() != 1)
                throw new InvalidOperationException($"Not a single value for key: {key}");
            
            return values.First();
        }

        /// <summary>
        /// Get a relation from an entity package
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static EntityRelation GetRelation(this EntityPackage entity, string type)
        {
            var relations= FindRelations(entity, type);

            if(relations.Count() != 1)
                throw new InvalidOperationException($"Not a single relation for type: {type}");
            
            return relations.First();
        }

        /// <summary>
        /// See if a package has a value (not necessarily singular)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasValue(this EntityPackage entity, string key)
        {
            return FindValues(entity, key).Any();
        }

        /// <summary>
        /// See if a package has a relation (not necessarily singluar)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasRelation(this EntityPackage entity, string type)
        {
            return FindRelations(entity, type).Any();
        }

        /// <summary>
        /// Create a "new" copy of the given entity
        /// </summary>
        /// <param name="entity"></param>
        public static EntityPackage NewCopy(this EntityPackage entity)
        {
            var newEntity = new EntityPackage(entity);
            newEntity.Entity.id = 0;
            newEntity.Values.ForEach(x => x.id = 0);
            newEntity.Relations.ForEach(x => x.id = 0);
            return newEntity;
        }
    }
}