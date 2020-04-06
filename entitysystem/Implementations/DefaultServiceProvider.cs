using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Randomous.EntitySystem.Implementations
{
    public class DefaultServiceProvider
    {
        public void AddDefaultServices(IServiceCollection services, Action<DbContextOptionsBuilder> buildContext, Action<DbContext> modifyContext = null)
        {
            services.AddSingleton(new GeneralHelper());
            services.AddTransient<IEntitySearcher, EntitySearcher>();
            services.AddTransient<IEntityQueryable, EntityQueryableEfCore>();
            services.AddTransient<ISignaler<EntityBase>, SignalSystem<EntityBase>>();
            services.AddTransient<IEntityProvider, EntityProvider>();
            services.AddDbContext<BaseEntityContext>(buildContext);
            services.AddScoped<DbContext, BaseEntityContext>(s =>
            {
                var d = (BaseEntityContext)s.GetService(typeof(BaseEntityContext));

                if(modifyContext != null)
                    modifyContext(d);

                return d;
            });
        }
    }
}