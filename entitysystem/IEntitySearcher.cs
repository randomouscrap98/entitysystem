using System.Linq;

namespace Randomous.EntitySystem
{
    /// <summary>
    /// Tools for modifying queryable sets with entity searches (could be any kind of query)
    /// </summary>
    public interface IEntitySearcher
    {
        IQueryable<T> ApplyFinal<T>(IQueryable<T> query, EntitySearchBase search) where T : EntityBase;
        IQueryable<T> ApplyGeneric<T>(IQueryable<T> query, EntitySearchBase search, bool finalize = true) where T : EntityBase;
        IQueryable<Entity> ApplyEntitySearch(IQueryable<Entity> query, EntitySearch search, bool finalize = true);
        IQueryable<EntityValue> ApplyEntityValueSearch(IQueryable<EntityValue> query, EntityValueSearch search, bool finalize = true);
        IQueryable<EntityRelation> ApplyEntityRelationSearch(IQueryable<EntityRelation> query, EntityRelationSearch search, bool finalize = true);
    }
}