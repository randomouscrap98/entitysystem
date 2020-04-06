using Randomous.EntitySystem.Implementations;
using Xunit;

namespace Randomous.EntitySystem.test
{
    public class TestUnitTestBase : UnitTestBase
    {
        [Fact]
        public void TestCreateService()
        {
            var provider = CreateService<EntityProvider>();
            Assert.NotNull(provider);
        }
    }
}