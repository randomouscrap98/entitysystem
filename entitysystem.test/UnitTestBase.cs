using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;

namespace entitysystem.test
{
    public class UnitTestBase
    {
        public IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole().AddSerilog(new LoggerConfiguration().WriteTo.File($"{GetType()}.txt").CreateLogger()));
            services.AddTransient<IEntitySearcher, EntitySearcher>();
            services.AddTransient<IEntityProvider, EntityProviderMemory>();
            return services;
        }

        public T CreateService<T>()
        {
            var services = CreateServices();
            var provider = services.BuildServiceProvider();
            return (T)ActivatorUtilities.CreateInstance(provider, typeof(T));
        }

        [Fact]
        public void TestCreateService()
        {
            var provider = CreateService<EntityProviderMemory>();
            Assert.NotNull(provider);
        }
    }
}
