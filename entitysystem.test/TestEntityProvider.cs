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
    public abstract class BaseTestEntityProvider : UnitTestBase
    {
        protected IEntityProvider provider;

        public Entity NewEntity()
        {
            return new Entity()
            {
                createDate = DateTime.Now,
                type = "Yeah",
                name = "The ENtity",
                content = "A lot of content comes from here!"
            };
        }

        public EntityValue NewValue()
        {
            return new EntityValue()
            {
                createDate = DateTime.Now,
                key = "someKey",
                value = "some value"
            };
        }

        public EntityRelation NewRelation()
        {
            return new EntityRelation()
            {
                createDate = DateTime.Now,
                entityId1 = 5, //Just some value; foreign key constraints shouldn't be present I think!!
                type = "someType",
                value = "some relation value"
            };
        }

        public EntityPackage NewPackage()
        {
            var package = new EntityPackage() { Entity = NewEntity()};
            var value = NewValue();
            var relation = NewRelation();
            provider.AddValues(package, value);
            provider.AddRelations(package, relation);
            return package;
        }

        [Fact]
        public void SimpleProviderWriteTest()
        {
            //Can we insert objects and get them out?
            provider.WriteAsync(NewEntity()).Wait();
        }

        [Fact]
        public void SimpleProviderReadTest()
        {
            //Can we insert objects and get them out?
            var entity = NewEntity();
            provider.WriteAsync(entity).Wait();
            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(entity, entities.First());
            //NOTE: this also assumes entity id is written!
        }

        [Fact]
        public void SimpleProviderUpdateTest()
        {
            var entity = NewEntity();
            provider.WriteAsync(entity).Wait();
            entity.type = "NONEOFYOURBUSINESS";
            provider.WriteAsync(entity).Wait(); //This SHOULD be just an update
            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Single(entities);
            Assert.Equal(entity, entities.First()); //assume this works correctly (is it safe to assume?)
        }

        //WARN: CAN'T TRACK SAME THING FROM MULTIPLE PLACES!!! How will
        //multithreading work?? 

        //[Fact]
        //public void NonTrackedUpdateTest()
        //{
        //    var entity = NewSingleEntity();
        //    provider.WriteAsync<Entity>(new[] {entity}).Wait();
        //    var newEntity = NewSingleEntity();
        //    newEntity.id = entity.id;
        //    newEntity.type = "NONEOFYOURBUSINESS";
        //    provider.WriteAsync<Entity>(new[] {newEntity}).Wait(); //This SHOULD be just an update
        //    var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
        //    Assert.Single(entities);
        //    Assert.Equal(newEntity, entities.First()); //assume this works correctly (is it safe to assume?)
        //    //Assert.Equal(entities.First().createDate)
        //}

        [Fact]
        public void ProviderMultiWriteTest()
        {
            //Can we insert objects and get them out?
            for(var i = 0; i < 10; i++)
                provider.WriteAsync(NewEntity()).Wait();

            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(10, entities.Count);

            //Now try to insert them in a batch
            var writeEntries = new List<Entity>();

            for(var i = 0; i < 10; i++)
                writeEntries.Add(NewEntity());

            provider.WriteAsync(writeEntries.ToArray()).Wait();
            entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(20, entities.Count);
        }

        [Fact]
        public void SimpleProviderDeleteTest()
        {
            var entity = NewEntity();
            provider.WriteAsync(entity).Wait();
            var entities = provider.GetEntitiesAsync(new EntitySearch() {}).Result;
            Assert.Equal(entity, entities.First());
            provider.DeleteAsync(entity).Wait();
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
            Assert.False(task.IsCompleted);

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
        public virtual void SingleEntityPackageWrite()
        {
            //See if you can even write a SINGLE entity within a package
            provider.WriteAsync(new EntityPackage() { Entity = NewEntity()}).Wait();
        }

        [Fact]
        public virtual void SimpleEntityPackageWrite()
        {
            //This creates a package with 1 value and 1 relation
            var package = NewPackage();
            provider.WriteAsync(package).Wait();
        }

        [Fact]
        public virtual void SimpleEntityPackageRead()
        {
            //This creates a package with 1 value and 1 relation
            var package = NewPackage();
            provider.WriteAsync(package).Wait();
            Assert.True(package.Entity.id > 0);

            var result = provider.GetEntitiesAsync(new EntitySearch()).Result;
            Assert.Single(result);
            Assert.Equal(package.Entity, result.First());
        }

        [Fact]
        public virtual void SimpleEntityPackageExpand()
        {
            //This creates a package with 1 value and 1 relation
            var package = NewPackage();
            provider.WriteAsync(package).Wait();
            Assert.True(package.Entity.id > 0);

            var result = provider.GetEntitiesAsync(new EntitySearch()).Result;
            Assert.Single(result);
            Assert.Equal(package.Entity, result.First());

            var expanded = provider.ExpandAsync(result.First()).Result;
            Assert.Single(expanded);
            Assert.Equal(package, expanded.First());
        }

        [Fact]
        public virtual void EntityPackageExpandRewrite()
        {
            //This creates a package with 1 value and 1 relation
            var package = NewPackage();
            provider.WriteAsync(package).Wait();

            var result = provider.GetEntitiesAsync(new EntitySearch()).Result;
            var expanded = provider.ExpandAsync(result.First()).Result.First();
            expanded.Values.First().Value.First().value = "lolButts";
            provider.WriteAsync(expanded).Wait();

            result = provider.GetEntitiesAsync(new EntitySearch()).Result;
            var expanded2 = provider.ExpandAsync(result.First()).Result;
            Assert.Single(expanded2);
            Assert.Equal(expanded, expanded2.First());
        }

        [Fact]
        public virtual void EntityPackageBiggerExpand()
        {
            //This creates a package with 1 value and 1 relation
            var package = NewPackage();
            provider.AddValues(package, NewValue(), NewValue(), NewValue());
            provider.AddRelations(package, NewRelation(), NewRelation(), NewRelation());
            provider.WriteAsync(package).Wait();
            Assert.True(package.Entity.id > 0);

            var result = provider.GetEntitiesAsync(new EntitySearch()).Result;
            Assert.Single(result);
            Assert.Equal(package.Entity, result.First());

            var expanded = provider.ExpandAsync(result.First()).Result;
            Assert.Single(expanded);
            Assert.Equal(package, expanded.First());
        }
    }

    public class TestEntityProviderEfCore : BaseTestEntityProvider
    {
        public TestEntityProviderEfCore()
        {
            var p = CreateService<EntityProviderEfCore>();
            p.context.Database.EnsureCreated();
            provider = p;
        }
    }

    public class TestEntityProviderMemory : BaseTestEntityProvider
    {
        public TestEntityProviderMemory()
        {
            provider = CreateService<EntityProviderMemory>();
        }

        //When I want to test a function, this is what I have to do. Stupid... unit testing.
        [Fact]
        public override void ListenLaterTest() { base.ListenLaterTest(); }

        [Fact]
        public override void SimpleEntityPackageExpand() { base.SimpleEntityPackageExpand(); }
    }
}