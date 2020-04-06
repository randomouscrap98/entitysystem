//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//
//namespace Randomous.EntitySystem.Implementations
//{
//    public class EntityExpander : IEntityExpander
//    {
//        protected ILogger<EntityExpander> logger;
//        protected GeneralHelper helper;
//
//        public EntityExpander(ILogger<EntityExpander> logger, GeneralHelper helper)
//        {
//            this.logger = logger;
//            this.helper = helper;
//        }
//
//        protected string EntityValueKey(EntityValue value) { return value.key; }
//        protected string EntityRelationKey(EntityRelation relation) { return relation.type; }
//
//        public void AddValues(EntityPackage package, params EntityValue[] values)
//        {
//            helper.MagicSort(values, EntityValueKey, package.Values);
//        }
//
//        public void AddRelations(EntityPackage package, params EntityRelation[] relations)
//        {
//            helper.MagicSort(relations, EntityRelationKey, package.ParentRelations);
//        }
//
//        public async Task<List<EntityPackage>> ExpandAsync(IEntityQueryable queryable, params Entity[] entities)
//        {
//            var results = new List<EntityPackage>();
//            var ids = entities.Select(x => x.id).ToList();
//
//            var valueSearch = new EntityValueSearch() { EntityIds = ids };
//            var relationSearch = new EntityRelationSearch() {EntityIds2 = ids };
//
//            //This is actually where the heavy lifting is.
//            var allValues = await queryable.GetList(queryable.GetQueryable<EntityValue>().Where(x => ids.Contains(x.entityId))); 
//            var allRelations = await queryable.GetList(queryable.GetQueryable<EntityRelation>().Where(x => ids.Contains(x.entityId2)));
//
//            foreach(var entity in entities)
//            {
//                var package = new EntityPackage();
//                package.Entity = entity;
//                AddValues(package, allValues.Where(x => x.entityId == entity.id).ToArray());
//                AddRelations(package, allRelations.Where(x => x.entityId2 == entity.id).ToArray());
//                results.Add(package);
//            }
//
//            return results;
//        }
//
//        public async Task WriteAsync(IEntityQueryable queryable, params EntityPackage[] entities)
//        {
//            foreach(var entity in entities)
//            {
//                logger.LogDebug($"Writing entity package for {entity.Entity}");
//
//                await queryable.WriteAsync(entity.Entity);
//
//                //Warn: NOW that the entity has been written, it should have an id. MAKE SURE the relations and whatnot are linked to this!
//                var values = entity.Values.SelectMany(x => x.Value).ToArray();
//                var relations = entity.ParentRelations.SelectMany(x => x.Value).ToArray();
//
//                foreach(var value in values)
//                    if(value.entityId == 0)
//                        value.entityId = entity.Entity.id;
//
//                foreach(var relation in relations)
//                    if(relation.entityId2 == 0)
//                        relation.entityId2 = entity.Entity.id;
//
//                await queryable.WriteAsync(values);
//                await queryable.WriteAsync(relations);
//            }
//        }
//    }
//}