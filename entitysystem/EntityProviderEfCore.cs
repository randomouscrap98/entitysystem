using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem
{
    public class EntityProviderEfCore : EntityProviderBase, IEntityProvider
    {
        public DbContext context;

        public EntityProviderEfCore(EntityProviderBaseServices services, DbContext context)
        {
            this.services = services;
            this.context = context;
        }

        protected override IQueryable<E> GetQueryable<E>() { return context.Set<E>(); }
        protected override async Task<List<E>> GetList<E>(IQueryable<E> query) { return await query.ToListAsync(); }

        public async Task DeleteAsync<E>(params E[] items) where E : EntityBase
        {
            services.Logger.LogTrace($"DeleteAsync called for {items.Count()} {typeof(E).Name} items");
            context.RemoveRange(items);
            await context.SaveChangesAsync();
            FinalizeWrite(items);
        }

        public override async Task WriteAsync<E>(params E[] items)
        {
            //Yes, we let efcore do all the work. if something weird happens... oh well. this class
            //isn't meant for safety... I think?
            services.Logger.LogTrace($"WriteAsync called for {items.Count()} {typeof(E).Name} items");
            context.UpdateRange(items);
            await context.SaveChangesAsync();
            FinalizeWrite(items);
        }
    }
}