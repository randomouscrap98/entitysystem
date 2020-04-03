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
            Assert.Equal(new EntityBase(), new EntityBase());
            Assert.Equal(new EntityBase() { id = 1 }, new EntityBase() { id = 1 });
            Assert.Equal(new EntityBase() { createDate = now }, new EntityBase() { createDate = now });
        }

        [Fact]
        public void TestEntityBaseInequality()
        {
            Assert.NotEqual(new EntityBase() { id = 8 }, new EntityBase() { id = 9 });
            Assert.NotEqual(
                new EntityBase() { createDate = DateTime.Now },
                new EntityBase() { createDate = DateTime.Now.AddMinutes(1) });
        }

        [Fact]
        public void TestEntityEquality()
        {
            //Screw these stupid individual unit tests. It's so much easier to just have it in the same place.
            Assert.Equal(new Entity(), new Entity());
            Assert.Equal(new Entity() { name = "yes" }, new Entity() { name = "yes" });
            Assert.Equal(new Entity() { content = "yes" }, new Entity() { content = "yes" });
            Assert.Equal(new Entity() { type = "yes" }, new Entity() { type = "yes" });
        }

        [Fact]
        public void TestEntityInequality()
        {
            Assert.NotEqual(new Entity() { name = "yes" }, new Entity() { name = "no" });
            Assert.NotEqual(new Entity() { content = "yes" }, new Entity() { content = "no" });
            Assert.NotEqual(new Entity() { type = "yes" }, new Entity() { type = "no" });
        }
    }
}