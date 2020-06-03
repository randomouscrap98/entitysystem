using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public void RandomSort()
        {
            List<Entity> entities = new List<Entity>();
            for(var i = 0; i < 100; i++)
                entities.Add(NewEntity());

            provider.WriteAsync(entities.ToArray()).Wait();


            var search = new EntitySearch();
            search.Sort = "random";

            var result = provider.GetEntitiesAsync(search).Result;
            var result2 = provider.GetEntitiesAsync(search).Result;

            Assert.False(result.Select(x => x.id).SequenceEqual(result2.Select(x => x.id)));
        }

        [Fact]
        public void ListenTest()
        {
            //We're listening on empty
            var task = provider.ListenAsync<Entity>(1, (q) => q.Where(e => e.id > 0), TimeSpan.FromMinutes(1), CancellationToken.None);
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
            var task = provider.ListenAsync<Entity>(1, (q) => q.Where(e => e.id > 5), TimeSpan.FromMinutes(1), CancellationToken.None);
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

        [Fact]
        public void ListenInstant()
        {
            //We're listening with something available
            var entity = NewEntity();
            provider.WriteAsync(entity).Wait();

            var task = provider.ListenAsync<Entity>(1, (q) => q.Where(e => e.id > 0), TimeSpan.FromMinutes(1), CancellationToken.None);

            var result = AssertWait(task);
            Assert.Single(result);
            Assert.Equal(entity, result.First());
        }
    }
}