using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace entitysystem
{
    public class EntityProviderEfCore : IEntityProvider
    {
        protected ILogger logger;
        protected IEntitySearcher searcher;
        public DbContext context;

        public EntityProviderEfCore(ILogger<EntityProviderEfCore> logger, IEntitySearcher searcher, DbContext context)
        {
            this.searcher = searcher;
            this.logger = logger;
            this.context = context;
        }

        public async Task<List<Entity>> GetEntitiesAsync(EntitySearch search)
        {
            logger.LogTrace("GetEntitiesAsync called");
            return await searcher.ApplyEntitySearch(context.Set<Entity>(), search).ToListAsync();
        }

        public async Task<List<EntityRelation>> GetEntityRelationsAsync(EntityRelationSearch search)
        {
            logger.LogTrace("GetEntityRelationsAsync called");
            return await searcher.ApplyEntityRelationSearch(context.Set<EntityRelation>(), search).ToListAsync();
        }

        public async Task<List<EntityValue>> GetEntityValuesAsync(EntityValueSearch search)
        {
            logger.LogTrace("GetEntityValuesAsync called");
            return await searcher.ApplyEntityValueSearch(context.Set<EntityValue>(), search).ToListAsync();
        }

        //public async Task<List<E>> FindEntities<E>(IEnumerable<E> entities) where E : EntityBase
        //{
        //    var search = new EntitySearchBase() 
        //    {
        //        Ids = entities.Where(x => x.id > 0).Select(x => x.id).ToList()
        //    };

        //    return await searcher.ApplyGeneric<E>(context.Set<E>(), search).ToListAsync();
        //}

        ///// <summary>
        ///// Loop through entities and store ones that are just updates. Forces create date to stay the same.
        ///// </summary>
        ///// <param name="entities"></param>
        ///// <typeparam name="E"></typeparam>
        ///// <returns></returns>
        //public async Task<List<E>> StoreUpdates<E>(IEnumerable<E> entities) where E : EntityBase
        //{
        //    List<E> newEntities = new List<E>();

        //    //Go find all the entities that have nonzero ids. These are updates.
        //    var search = new EntitySearchBase() 
        //    {
        //        Ids = entities.Where(x => x.id > 0).Select(x => x.id).ToList()
        //    };

        //    //Can't update when there are duplicates given; which ones do we update??
        //    if(search.Ids.Count != search.Ids.Distinct().Count())
        //        throw new InvalidOperationException($"Duplicate update {typeof(E)}!");

        //    var found = await searcher.ApplyGeneric<E>(context.Set<E>(), search).ToListAsync();

        //    //Oh, not all were found. that means the user gave us faulty entities, don't do updates (for instance, an id
        //    //was given that doesn't actually exist yet)
        //    if(!(search.Ids.OrderBy(x => x).SequenceEqual(found.Select(x => x.id).OrderBy(x => x))))
        //        throw new InvalidOperationException("Tried to update entities that don't exist");

        //    //Loop over EVERY entity given. Save ones that aren't "updates" for later. Assume at this point that "found"
        //    //is valid and checked, so no error checking here.
        //    foreach(var entity in entities)
        //    {
        //        var existing = found.FirstOrDefault(x => x.id == entity.id);

        //        if(existing != null)
        //        {
        //            entity.createDate = 
        //        }
        //        else
        //        {
        //            newEntities.Add(entity);
        //        }
        //    }

        //    return newEntities;
        //}

        public async Task WriteEntitiesAsync(IEnumerable<Entity> entities)
        {
            //Yes, we let efcore do all the work. if something weird happens... oh well. this class
            //isn't meant for safety... I think?
            context.UpdateRange(entities);
            await context.SaveChangesAsync();
        }

        public async Task WriteEntityRelationsAsync(IEnumerable<EntityRelation> relations)
        {
            context.UpdateRange(relations);
            await context.SaveChangesAsync();
        }

        public async Task WriteEntityValuesAsync(IEnumerable<EntityValue> values)
        {
            context.UpdateRange(values);
            await context.SaveChangesAsync();
        }
    }
}