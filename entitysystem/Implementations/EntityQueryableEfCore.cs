using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem.Implementations
{
    public class EntityQueryableEfCore : IEntityQueryable
    {
        public DbContext context;
        protected ILogger<EntityQueryableEfCore> logger;

        public EntityQueryableEfCore(ILogger<EntityQueryableEfCore> logger, DbContext context)
        {
            this.context = context;
            this.logger = logger;
        }

        public IQueryable<E> GetQueryable<E>() where E : EntityBase { return context.Set<E>(); } 
        public Task<List<E>> GetListAsync<E>(IQueryable<E> query) { return query.ToListAsync(); }
        public Task<List<E>> GetAllAsync<E>() where E : EntityBase { return GetListAsync(GetQueryable<E>()); }

        public Task DeleteAsync<E>(params E[] items) where E : EntityBase
        {
            logger.LogTrace($"DeleteAsync called for {items.Count()} {typeof(E).Name} items");
            context.RemoveRange(items);
            return context.SaveChangesAsync();
        }

        public Task WriteAsync<E>(params E[] items) where E : EntityBase
        {
            //Yes, we let efcore do all the work. if something weird happens... oh well. this class
            //isn't meant for safety... I think?
            logger.LogTrace($"WriteAsync called for {items.Count()} {typeof(E).Name} items");
            context.UpdateRange(items);
            return context.SaveChangesAsync();
        }
    }
}