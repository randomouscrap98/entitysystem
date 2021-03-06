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
        public SqliteConnection connection;

        protected DefaultServiceProvider serviceProvider;

        public UnitTestBase()
        {
            connection = new SqliteConnection("Data Source=:memory:;");
            connection.Open();
            serviceProvider = new DefaultServiceProvider();
        }

        public void Dispose()
        {
            try { connection.Close(); }
            catch(Exception) { }
        }

        public virtual IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => 
            {
                var seriConfig = new LoggerConfiguration().WriteTo.File($"{GetType()}.txt");
                seriConfig.MinimumLevel.Verbose();
                configure.AddSerilog(seriConfig.CreateLogger());
                configure.AddDebug();
                configure.SetMinimumLevel(LogLevel.Trace);
            });
            serviceProvider.AddDefaultServices(
                services, 
                options => options.UseSqlite(connection).EnableSensitiveDataLogging(true),
                    d => d.Database.EnsureCreated());
            services.AddSingleton(new EntityQueryableEfCoreConfig() { ConcurrentAccess = 1}); //only want single access for tests

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

        public void AssertNotWait(Task task)
        {
            Assert.False(task.Wait(10));
            Assert.False(task.IsCompleted);
        }

        protected void AssertResultsEqual<T>(IEnumerable<T> expected, IEnumerable<T> result)
        {
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.ToHashSet(), result.ToHashSet());
        }

        protected void AssertThrows<E>(Action action) where E : Exception
        {
            try
            {
                action();
                Assert.Equal("This should've thrown an exception", "It didn't");
            }
            catch(Exception ex)
            {
                Assert.True(ex is E, $"exception is type {typeof(E).Name}");
            }
        }
    }
}
