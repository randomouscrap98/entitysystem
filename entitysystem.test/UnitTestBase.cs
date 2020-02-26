using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;

namespace entitysystem.test
{
    public class UnitTestBase : IDisposable
    {
        public List<SqliteConnection> connections = new List<SqliteConnection>();

        public void Dispose()
        {
            foreach(var con in connections)
            {
                try { con.Close(); }
                catch(Exception) { }
            }
        }

        public IServiceCollection CreateServices()
        {
            //Whhyyyy am I doing it like this.
            var connection = new SqliteConnection("Data Source=:memory:;");
            connection.Open();
            connections.Append(connection);

            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole().AddSerilog(new LoggerConfiguration().WriteTo.File($"{GetType()}.txt").CreateLogger()));
            services.AddTransient<IEntitySearcher, EntitySearcher>();
            services.AddTransient<IEntityProvider, EntityProviderEfCore>();
            services.AddDbContext<BaseEntityContext>(options => options.UseSqlite(connection).EnableSensitiveDataLogging(false));
            services.AddScoped<DbContext, BaseEntityContext>();
            /*services.AddSingleton<DbContext, BaseEntityContext>(pr => 
            {
                var ctx = pr.GetService<BaseEntityContext>();
                ctx.Database.EnsureCreated();
                return ctx;
            });*/
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
            var provider = CreateService<EntityProviderEfCore>();
            Assert.NotNull(provider);
        }

        protected void AssertResultsEqual<T>(IEnumerable<T> expected, IEnumerable<T> result)
        {
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.ToHashSet(), result.ToHashSet());
        }
    }
}
