using System;
using Xunit;

namespace entitysystem.test
{
    public class EqualityTest : UnitTestBase
    {
        [Fact]
        public void TestEntityBaseEmptyEquality()
        {
            Assert.Equal(new EntityBase(), new EntityBase());
        }

        [Fact]
        public void TestEntityBaseIdEquality()
        {
            Assert.Equal(new EntityBase() { id = 1 }, new EntityBase() { id = 1 });
        }

        [Fact]
        public void TestEntityBaseIdInequality()
        {
            Assert.NotEqual(new EntityBase() { id = 8 }, new EntityBase() { id = 9 });
        }

        [Fact]
        public void TestEntityBaseCreateDateEquality()
        {
            var now = DateTime.Now;
            Assert.Equal(new EntityBase() { createDate = now }, new EntityBase() { createDate = now });
        }

        [Fact]
        public void TestEntityBaseCreateDateInequality()
        {
            Assert.NotEqual(
                new EntityBase() { createDate = DateTime.Now },
                new EntityBase() { createDate = DateTime.Now.AddMinutes(1) });
        }

        [Fact]
        public void TestEntityEmptyEquality()
        {
            Assert.Equal(new Entity(), new Entity());
        }

        [Fact]
        public void TestEntityNameEquality()
        {
            Assert.Equal(new Entity() { name = "yes" }, new Entity() { name = "yes" });
        }

        [Fact]
        public void TestEntityNameInequality()
        {
            Assert.NotEqual(new Entity() { name = "yes" }, new Entity() { name = "no" });
        }

        [Fact]
        public void TestEntityContentEquality()
        {
            Assert.Equal(new Entity() { content = "yes" }, new Entity() { content = "yes" });
        }

        [Fact]
        public void TestEntityContentInequality()
        {
            Assert.NotEqual(new Entity() { content = "yes" }, new Entity() { content = "no" });
        }

        [Fact]
        public void TestEntityTypeEquality()
        {
            Assert.Equal(new Entity() { type = "yes" }, new Entity() { type = "yes" });
        }

        [Fact]
        public void TestEntityTypeInequality()
        {
            Assert.NotEqual(new Entity() { type = "yes" }, new Entity() { type = "no" });
        }
    }
}