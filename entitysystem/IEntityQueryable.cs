using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Randomous.EntitySystem
{
    /// <summary>
    /// An interface for providing queryable entities. Could be a dbcontext, could be memory, who knows.
    /// </summary>
    public interface IEntityQueryable
    {
        /// <summary>
        /// The completion of a query into a concrete list. Run this against your completed query, NOT
        /// guaranteed that calling ToList() on your query will work!
        /// </summary>
        /// <param name="query"></param>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        Task<List<E>> GetList<E>(IQueryable<E> query);

        /// <summary>
        /// Quick shortcut to get all of type E
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        Task<List<E>> GetAll<E>() where E : EntityBase;

        /// <summary>
        /// The base queryable for all E, run your queries against this
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        IQueryable<E> GetQueryable<E>() where E : EntityBase;

        Task WriteAsync<E>(params E[] entities) where E : EntityBase;

        Task DeleteAsync<E>(params E[] items) where E : EntityBase;
    }
}