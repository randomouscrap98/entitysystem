using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomous.EntitySystem;

namespace Randomous.EntitySystem.Extensions
{
    public static class EntityProviderWrapperExtensions
    {
        //public static ILogger Logger = null;

        /// <summary>
        /// Write an entire entity wrapper as-is, setting all associated ids.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static async Task WriteAsync(this IEntityProvider provider, EntityPackage entity)
        {
            var allWrite = new List<EntityBase>();
            allWrite.AddRange(entity.Values);
            allWrite.AddRange(entity.Relations);

            if(entity.Entity.id > 0)
            {
                //If this is an update entity, it's easy. Just save everything all
                //at once, making the whole dang thing atomic
                allWrite.Add(entity.Entity);
                await provider.WriteAsync(allWrite.ToArray());
            }
            else
            {
                //If this is a NEW entity, try to write it first then update the values and relations.
                //If something goes wrong with the values/relations, delete the entity we added.
                await provider.WriteAsync(entity.Entity);

                try
                {
                    entity.Values.ForEach(x => x.entityId = entity.Entity.id);
                    entity.Relations.ForEach(x => x.entityId2 = entity.Entity.id); //Assume relations are all parents. a user has perms ON this entity, a category OWNS this entity, etc.
                    await provider.WriteAsync(allWrite.ToArray());
                }
                catch
                {
                    await provider.DeleteAsync(entity.Entity);
                    throw;
                }

            }
        }

        /// <summary>
        /// Link a queryable entity list with values/relations to produce a list of packaged entities
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static async Task<List<EntityPackage>> LinkAsync(this IEntityProvider provider, IQueryable<Entity> queryable)
        {
            //Logger?.LogTrace("LinkAsync called on entity queryable");

            //Performance test this sometime
            var entities = await provider.GetListAsync(queryable);
            var ids = entities.Select(x => x.id).ToList();
            var values = await provider.GetEntityValuesAsync(new EntityValueSearch() { EntityIds = ids }); //Will this stuff be safe?
            var relations = await provider.GetEntityRelationsAsync(new EntityRelationSearch() { EntityIds2 = ids }); //Will this stuff be safe?

            return entities.Select(x => new EntityPackage()
            {
                Entity = x,
                Values = values.Where(y => y.entityId == x.id).ToList(),
                Relations = relations.Where(y => y.entityId2 == x.id).ToList()
            }).ToList();

            //var bigGroup = from e in queryable
            //               join v in provider.GetQueryable<EntityValue>() on e.id equals v.entityId into evs
            //               from v in evs.DefaultIfEmpty()
            //               join r in provider.GetQueryable<EntityRelation>() on e.id equals r.entityId2 into evrs
            //               from r in evrs.DefaultIfEmpty()
            //               select new { Entity = e, Value = v, Relation = r};
            
            //var grouping = (await provider.GetListAsync(bigGroup)).ToLookup(x => x.Entity.id);

            //return grouping.Select(x => new EntityPackage()
            //{
            //    Entity = x.First().Entity,
            //    Values = x.Select(y => y.Value).ToList(),
            //    Relations = x.Select(y => y.Relation).ToList()
            //}).ToList();
        }

        /// <summary>
        /// Search for entities, return them packaged
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static Task<List<EntityPackage>> GetEntityPackagesAsync(this IEntityProvider provider, EntitySearch search)
        {
            return LinkAsync(provider, provider.ApplyEntitySearch(provider.GetQueryable<Entity>(), search));
        }
    }
}