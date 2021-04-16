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
            services.AddSingleton(typeof(ISignaler<>), typeof(SignalSystem<>)); //No, this is setup for a STANDARD use case which would WANT a single signaller
            services.AddSingleton<EntityQueryableEfCoreConfig>(); //Just get some defaults in there...

            services.AddTransient<IEntitySearcher, EntitySearcher>();
            services.AddTransient<IEntityQueryable, EntityQueryableEfCore>();
            services.AddTransient<IEntityProvider, EntityProvider>();
            services.AddDbContext<BaseEntityContext>(buildContext, ServiceLifetime.Transient, ServiceLifetime.Transient);
            services.AddTransient<DbContext, BaseEntityContext>(s =>
            {
                var d = (BaseEntityContext)s.GetService(typeof(BaseEntityContext));

                if(modifyContext != null)
                    modifyContext(d);

                return d;
            });
        }
    }
}