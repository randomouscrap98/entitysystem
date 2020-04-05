using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomous.EntitySystem.Implementations;
using Serilog;
using Xunit;

namespace Randomous.EntitySystem.test
{
    public class UnitTestBase : IDisposable
    {
        public List<SqliteConnection> connections = new List<SqliteConnection>();
        public string SqliteConnectionString = "Data Source=:memory:;";

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
            var connection = new SqliteConnection(SqliteConnectionString);
            connection.Open();
            connections.Append(connection);

            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddSerilog(new LoggerConfiguration().WriteTo.File($"{GetType()}.txt").CreateLogger()));
            services.AddSingleton(new GeneralHelper());
            services.AddTransient<IEntitySearcher, EntitySearcher>();
            services.AddTransient<IEntityProvider, EntityProviderEfCore>();
            services.AddTransient<EntityProviderBaseServices>();
            services.AddTransient<ISignaler<EntityBase>, SignalSystem<EntityBase>>();
            services.AddDbContext<BaseEntityContext>(options => options.UseSqlite(connection).EnableSensitiveDataLogging(true));
            services.AddScoped<DbContext, BaseEntityContext>();
            return services;
        }

        public T CreateService<T>()
        {
            var services = CreateServices();
            var provider = services.BuildServiceProvider();
            return (T)ActivatorUtilities.GetServiceOrCreateInstance(provider, typeof(T));
        }

        public T AssertWait<T>(Task<T> task)
        {
            Assert.True(task.Wait(2000));    //We should've gotten signaled. Give the test plenty of time to get the memo
            return task.Result;    //This won't wait at all if the previous came through
        }

        protected void AssertResultsEqual<T>(IEnumerable<T> expected, IEnumerable<T> result)
        {
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.ToHashSet(), result.ToHashSet());
        }
    }
}
