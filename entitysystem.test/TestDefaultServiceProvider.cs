using Xunit;

namespace Randomous.EntitySystem.test
{
    public class TestDefaultServiceProvider : UnitTestBase
    {
        [Fact]
        public void GetEntityProvider()
        {
            var provider = CreateService<IEntityProvider>();
            Assert.NotNull(provider);
        }
    }
}