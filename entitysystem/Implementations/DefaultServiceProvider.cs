using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Randomous.EntitySystem.Implementations
{
    public class DefaultServiceProvider
    {
        public void AddDefaultServices(IServiceCollection services, Action<DbContextOptionsBuilder> buildContext)
        {
            services.AddSingleton(new GeneralHelper());
            services.AddTransient<IEntitySearcher, EntitySearcher>();
            //services.AddTransient<IEntityProvider, EntityProviderEfCore>();
            services.AddTransient<IEntityQueryable, EntityQueryableEfCore>();
            services.AddTransient<EntityProviderBaseServices>(); //This is the service package for EntityPRoviderBase
            services.AddTransient<ISignaler<EntityBase>, SignalSystem<EntityBase>>();
            services.AddDbContext<BaseEntityContext>(buildContext);
            services.AddScoped<DbContext, BaseEntityContext>();
        }
    }
}