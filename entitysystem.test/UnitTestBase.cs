using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace entitysystem.test
{
    public class UnitTestBase
    {
        public IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(new EntitySearchHelper());
            services.AddTransient<IEntityProvider, EntityProviderMemory>();
            return services;
        }

        public IEntityProvider GetProvider()
        {
            return CreateService<IEntityProvider>();
        }

        public T CreateService<T>()
        {
            var services = CreateServices();
            var provider = services.BuildServiceProvider();
            return provider.GetService<T>();
        }

        [Fact]
        public void TestCreateService()
        {
            var provider = GetProvider();
            Assert.NotNull(provider);
        }
    }
}
