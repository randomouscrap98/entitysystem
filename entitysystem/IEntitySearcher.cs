using System.Linq;

namespace entitysystem
{
    /// <summary>
    /// Tools for modifying queryable sets with entity searches (could be any kind of query)
    /// </summary>
    public interface IEntitySearcher
    {
        IQueryable<Entity> ApplyEntitySearch(IQueryable<Entity> query, EntitySearch search);
        IQueryable<EntityValue> ApplyEntityValueSearch(IQueryable<EntityValue> query, EntityValueSearch search);
        IQueryable<EntityRelation> ApplyEntityRelationSearch(IQueryable<EntityRelation> query, EntityRelationSearch search);
    }
}