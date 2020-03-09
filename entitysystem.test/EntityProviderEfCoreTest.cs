using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Randomous.EntitySystem.test
{
    /// <summary>
    /// These are integration tests for EntityProvider. Should work mostly like it does in production
    /// </summary>
    /// <remarks>
    /// These tests are mostly to make sure the underlying system is performing as expected. The underlying 
    /// system is the (probably well-tested) entity framework core. I know this is silly and "improper" but
    /// I don't care: I want to make sure the provider can operate as expected: can you write to it and then 
    /// read what you wrote? That's about all I care about.
    /// </remarks>
    public class EntityProviderEfCoreTest: UnitTestBase
    {
        protected EntityProviderEfCore provider;

        public EntityProviderEfCoreTest()
        {
            provider = CreateService<EntityProviderEfCore>();
            provider.context.Database.EnsureCreated();  
        }

        public Entity NewSingleEntity()
        {
            return new Entity()
            {
                type = "Yeah",
                name = "The ENtity",
                content = "A lot of content comes from here!"
            };
        }

        [Fact]
        public void SimpleProviderWriteTest()
        {
            //Can we insert objects and get them out?
            provider.WriteAsync<Entity>(new[] {NewSingleEntity()}).Wait();
        }

        [Fact]
        public void SimpleProviderReadTest()
        {
            //Can we insert objects and get them out?
            var entity = NewSingleEntity();
            provider.WriteAsync<Entity>(new[] {entity}).Wait();
            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(entity, entities.First());
            //NOTE: this also assumes entity id is written!
        }

        [Fact]
        public void SimpleProviderUpdateTest()
        {
            var entity = NewSingleEntity();
            provider.WriteAsync<Entity>(new[] {entity}).Wait();
            entity.type = "NONEOFYOURBUSINESS";
            provider.WriteAsync<Entity>(new[] {entity}).Wait(); //This SHOULD be just an update
            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Single(entities);
            Assert.Equal(entity, entities.First()); //assume this works correctly (is it safe to assume?)
        }

        [Fact]
        public void ProviderMultiWriteTest()
        {
            //Can we insert objects and get them out?
            for(var i = 0; i < 10; i++)
                provider.WriteAsync<Entity>(new[] {NewSingleEntity()}).Wait();

            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(10, entities.Count);

            //Now try to insert them in a batch
            var writeEntries = new List<Entity>();

            for(var i = 0; i < 10; i++)
                writeEntries.Add(NewSingleEntity());

            provider.WriteAsync<Entity>(writeEntries).Wait();
            entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(20, entities.Count);
        }

        [Fact]
        public void SimpleProviderDeleteTest()
        {
            var entity = NewSingleEntity();
            provider.WriteAsync<Entity>(new[] {entity}).Wait();
            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(entity, entities.First());
            provider.DeleteAsync(new[] {entity}).Wait();
            entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Empty(entities);
        }

        [Fact]
        public void ListenTest()
        {
            //We're listening on empty
            var task = provider.ListenNewAsync<Entity>(0, TimeSpan.FromMinutes(1));
            Assert.False(task.IsCompleted);

            //Add a new entity. This should complete the above task.
            var entity = NewSingleEntity();
            provider.WriteAsync<Entity>(new[] {entity}).Wait();
            Assert.True(task.Wait(200));

            var result = task.Result;
            Assert.Single(result);
            Assert.Equal(entity, result.First());
        }

        [Fact]
        public void ListenLaterTest()
        {
            var task = provider.ListenNewAsync<Entity>(5, TimeSpan.FromMinutes(1));
            Assert.False(task.IsCompleted);

            for(var i = 0; i < 5; i++)
            {
                provider.WriteAsync<Entity>(new[] {NewSingleEntity()}).Wait();
                Assert.False(task.Wait(10));
                Assert.False(task.IsCompleted);
            }

            var entity = NewSingleEntity();
            provider.WriteAsync<Entity>(new[] {entity}).Wait();
            Assert.True(task.Wait(200));

            var result = task.Result;
            Assert.Single(result);
            Assert.Equal(entity, result.First());
        }
    }
}