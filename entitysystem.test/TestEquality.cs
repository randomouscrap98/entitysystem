using System;
using Xunit;

namespace Randomous.EntitySystem.test
{
    public class TestEquality : UnitTestBase
    {
        [Fact]
        public void TestEntityBaseEquality()
        {
            var now = DateTime.Now;
            Assert.Equal(new EntityBase(false), new EntityBase(false));
            Assert.Equal(new EntityBase(false) { id = 1 }, new EntityBase(false) { id = 1 });
            Assert.Equal(new EntityBase(false) { createDate = now }, new EntityBase(false) { createDate = now });
        }

        [Fact]
        public void TestEntityBaseInequality()
        {
            Assert.NotEqual(new EntityBase(false) { id = 8 }, new EntityBase(false) { id = 9 });
            Assert.NotEqual(
                new EntityBase(false) { createDate = DateTime.Now },
                new EntityBase(false) { createDate = DateTime.Now.AddMinutes(1) });
        }

        [Fact]
        public void TestEntityEquality()
        {
            //Screw these stupid individual unit tests. It's so much easier to just have it in the same place.
            Assert.Equal(new Entity(false), new Entity(false));
            Assert.Equal(new Entity(false) { name = "yes" }, new Entity(false) { name = "yes" });
            Assert.Equal(new Entity(false) { content = "yes" }, new Entity(false) { content = "yes" });
            Assert.Equal(new Entity(false) { type = "yes" }, new Entity(false) { type = "yes" });
        }

        [Fact]
        public void TestEntityInequality()
        {
            Assert.NotEqual(new Entity(false) { name = "yes" }, new Entity(false) { name = "no" });
            Assert.NotEqual(new Entity(false) { content = "yes" }, new Entity(false) { content = "no" });
            Assert.NotEqual(new Entity(false) { type = "yes" }, new Entity(false) { type = "no" });
        }
    }
}