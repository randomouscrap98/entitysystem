using Xunit;

namespace Randomous.EntitySystem.test
{
    public class TestUnitTestBase : UnitTestBase
    {
        [Fact]
        public void TestCreateService()
        {
            var provider = CreateService<EntityProviderEfCore>();
            Assert.NotNull(provider);
        }
    }
}