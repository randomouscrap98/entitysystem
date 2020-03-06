using System.IO;
using Xunit;

namespace Randomous.EntitySystem.test
{
    public class NonUnitTests : UnitTestBase
    {
        [Fact]
        public void CreateBaseEntityDatabase()
        {
            var dbFile = @"..\..\..\..\baseEntitySqlite.db";
            SqliteConnectionString = $"Data Source='{dbFile}';";

            var context = CreateService<BaseEntityContext>();
            context.Database.EnsureCreated();
            Assert.True(File.Exists(dbFile));
        }
    }
}