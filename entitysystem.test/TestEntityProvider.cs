using System;
using System.Linq;
using Randomous.EntitySystem.Implementations;
using Xunit;

namespace Randomous.EntitySystem.test
{
    public class TestEntityProvider : UnitTestBase
    {
        public EntityProvider provider; 

        public TestEntityProvider()
        {
            provider = CreateService<EntityProvider>();
        }

        protected Entity NewEntity()
        {
            return new Entity()
            {
                createDate = DateTime.Now,
                type = "Yeah",
                name = "The ENtity",
                content = "A lot of content comes from here!"
            };
        }

        [Fact]
        public void Regression_DateTimeCompare()
        {
            provider.WriteAsync(NewEntity(), NewEntity()).Wait();
            var entities = provider.GetEntitiesAsync(new EntitySearch() {CreateEnd = DateTime.Now.AddSeconds(10)}).Result;
            Assert.Equal(2, entities.Count);
        }

        [Fact]
        public void ListenTest()
        {
            //We're listening on empty
            var task = provider.ListenNewAsync<Entity>(0, TimeSpan.FromMinutes(1));
            AssertNotWait(task);

            //Add a new entity. This should complete the above task.
            var entity = NewEntity();
            provider.WriteAsync(entity).Wait();

            var result = AssertWait(task);
            Assert.Single(result);
            Assert.Equal(entity, result.First());
        }

        [Fact]
        public virtual void ListenLaterTest()
        {
            var task = provider.ListenNewAsync<Entity>(5, TimeSpan.FromMinutes(1));
            AssertNotWait(task);

            for(var i = 0; i < 5; i++)
            {
                provider.WriteAsync(NewEntity()).Wait();
                Assert.False(task.Wait(10)); //This might be bad...? Some "assert true" tests failed after even 200ms of waiting, so these could be incorrectly false
                Assert.False(task.IsCompleted);
            }

            var entity = NewEntity();
            provider.WriteAsync(entity).Wait();

            var result = AssertWait(task);
            Assert.Single(result);
            Assert.Equal(entity, result.First());
        }
    }
}