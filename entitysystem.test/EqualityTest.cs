using System;
using Xunit;

namespace entitysystem.test
{
    public class EqualityTest : UnitTestBase
    {
        [Fact]
        public void TestEntityBaseEmptyEquality()
        {
            var entity1 = new EntityBase();
            var entity2 = new EntityBase();
            Assert.Equal(entity1, entity2);
        }

        [Fact]
        public void TestEntityBaseIdEquality()
        {
            var entity1 = new EntityBase() { id = 1 };
            var entity2 = new EntityBase() { id = 1 };

            Assert.Equal(entity1, entity2);
        }

        [Fact]
        public void TestEntityBaseIdInequality()
        {
            var entity1 = new EntityBase() { id = 8 };
            var entity2 = new EntityBase() { id = 9 };

            Assert.NotEqual(entity1, entity2);
        }

        [Fact]
        public void TestEntityBaseCreateDateEquality()
        {
            var now = DateTime.Now;
            var entity1 = new EntityBase() { createDate = now };
            var entity2 = new EntityBase() { createDate = now };

            Assert.Equal(entity1, entity2);
        }

        [Fact]
        public void TestEntityBaseCreateDateInequality()
        {
            var entity1 = new EntityBase() { createDate = DateTime.Now };
            var entity2 = new EntityBase() { createDate = DateTime.Now.AddMinutes(1) };

            Assert.NotEqual(entity1, entity2);
        }
    }
}