using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        Task<List<E>> GetListAsync<E>(IQueryable<E> query);

        /// <summary>
        /// The completion of a query into a concrete max scalar.async Run this against your completed query.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="selector"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        Task<T> GetMaxAsync<T,E>(IQueryable<E> query, Expression<Func<E, T>> selector);

        /// <summary>
        /// Quick shortcut to get all of type E
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        Task<List<E>> GetAllAsync<E>() where E : EntityBase;

        /// <summary>
        /// The base queryable for all E, run your queries against this
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        Task<IQueryable<E>> GetQueryableAsync<E>() where E : EntityBase;

        Task WriteAsync<E>(params E[] entities) where E : EntityBase;

        Task DeleteAsync<E>(params E[] items) where E : EntityBase;
    }
}