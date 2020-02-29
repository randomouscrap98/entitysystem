using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace entitysystem.test
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
            provider.WriteEntitiesAsync(new[] {NewSingleEntity()}).Wait();
        }

        [Fact]
        public void SimpleProviderReadTest()
        {
            //Can we insert objects and get them out?
            var entity = NewSingleEntity();
            provider.WriteEntitiesAsync(new[] {entity}).Wait();
            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(entity, entities.First());
            //NOTE: this also assumes entity id is written!
        }

        [Fact]
        public void SimpleProviderUpdateTest()
        {
            var entity = NewSingleEntity();
            provider.WriteEntitiesAsync(new[] {entity}).Wait();
            entity.type = "NONEOFYOURBUSINESS";
            provider.WriteEntitiesAsync(new[] {entity}).Wait(); //This SHOULD be just an update
            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Single(entities);
            Assert.Equal(entity, entities.First()); //assume this works correctly (is it safe to assume?)
        }

        [Fact]
        public void ProviderMultiWriteTest()
        {
            //Can we insert objects and get them out?
            for(var i = 0; i < 10; i++)
                provider.WriteEntitiesAsync(new[] {NewSingleEntity()}).Wait();

            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(10, entities.Count);

            //Now try to insert them in a batch
            var writeEntries = new List<Entity>();

            for(var i = 0; i < 10; i++)
                writeEntries.Add(NewSingleEntity());

            provider.WriteEntitiesAsync(writeEntries).Wait();
            entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(20, entities.Count);
        }
    }
}