using System.IO;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Randomous.EntitySystem.test
{
    public class NonUnitTests : UnitTestBase
    {
        [Fact]
        public void CreateBaseEntityDatabase()
        {
            var dbFile = @"..\..\..\..\baseEntitySqlite.db";

            if(File.Exists(dbFile))
                File.Delete(dbFile);

            connection = new SqliteConnection($"Data Source='{dbFile}';");
            connection.Open();

            var context = CreateService<BaseEntityContext>();
            context.Database.EnsureCreated();
            Assert.True(File.Exists(dbFile));
        }
    }
}