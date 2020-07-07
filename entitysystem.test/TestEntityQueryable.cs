using System;
using System.Collections.Generic;
using System.Linq;
using Randomous.EntitySystem.Implementations;
using Xunit;

namespace Randomous.EntitySystem.test
{
    public abstract class TestEntityQueryableBase : UnitTestBase
    {
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

        protected IEntityQueryable queryable;

        public virtual void SimpleWriteTest()
        {
            //Can we insert objects and get them out?
            queryable.WriteAsync(NewEntity()).Wait();
        }

        public virtual void SimpleReadTest()
        {
            //Can we insert objects and get them out?
            var entity = NewEntity();
            queryable.WriteAsync(entity).Wait();
            var entities = queryable.GetAllAsync<Entity>().Result;
            Assert.Equal(entity, entities.First());
            //NOTE: this also assumes entity id is written!
        }

        public virtual void SimpleUpdateTest()
        {
            var entity = NewEntity();
            queryable.WriteAsync(entity).Wait();
            entity.type = "NONEOFYOURBUSINESS";
            queryable.WriteAsync(entity).Wait(); //This SHOULD be just an update
            var entities = queryable.GetAllAsync<Entity>().Result;
            Assert.Single(entities);
            Assert.Equal(entity, entities.First()); //assume this works correctly (is it safe to assume?)
        }

        public virtual void MultiWriteTest()
        {
            //Can we insert objects and get them out?
            for(var i = 0; i < 10; i++)
                queryable.WriteAsync(NewEntity()).Wait();

            var entities = queryable.GetAllAsync<Entity>().Result;
            Assert.Equal(10, entities.Count);

            //Now try to insert them in a batch
            var writeEntries = new List<Entity>();

            for(var i = 0; i < 10; i++)
                writeEntries.Add(NewEntity());

            queryable.WriteAsync(writeEntries.ToArray()).Wait();
            entities = queryable.GetAllAsync<Entity>().Result;
            Assert.Equal(20, entities.Count);
        }

        public virtual void SimpleDeleteTest()
        {
            var entity = NewEntity();
            queryable.WriteAsync(entity).Wait();
            var entities = queryable.GetAllAsync<Entity>().Result;
            Assert.Equal(entity, entities.First());
            queryable.DeleteAsync(entity).Wait();
            entities = queryable.GetAllAsync<Entity>().Result;
            Assert.Empty(entities);
        }

        public virtual void NonTrackedUpdateTest()
        {
            var entity = NewEntity();
            queryable.WriteAsync<Entity>(new[] {entity}).Wait();
            var newEntity = NewEntity();
            newEntity.id = entity.id;
            newEntity.type = "NONEOFYOURBUSINESS";
            queryable.WriteAsync<Entity>(new[] {newEntity}).Wait(); //This SHOULD be just an update
            var entities = queryable.GetAllAsync<Entity>().Result;
            Assert.Single(entities);
            Assert.Equal(newEntity, entities.First()); //assume this works correctly (is it safe to assume?)
            //Assert.Equal(entities.First().createDate)
        }

        public virtual void GetMaxAsync()
        {
            var writeEntries = new List<Entity>();

            for(var i = 0; i < 10; i++)
                writeEntries.Add(NewEntity());

            queryable.WriteAsync(writeEntries.ToArray()).Wait();

            Assert.True(queryable.GetMaxAsync(queryable.GetQueryable<Entity>(), x => x.id).Result >= 10);
        }
    }

    public class TestEntityQueryableEfCore : TestEntityQueryableBase
    {
        public TestEntityQueryableEfCore()
        {
            queryable = CreateService<EntityQueryableEfCore>();
        }

        [Fact] public override void SimpleReadTest() { base.SimpleReadTest(); }
        [Fact] public override void SimpleUpdateTest() { base.SimpleUpdateTest(); }
        [Fact] public override void SimpleWriteTest() { base.SimpleWriteTest(); }
        [Fact] public override void SimpleDeleteTest() { base.SimpleDeleteTest(); }
        [Fact] public override void MultiWriteTest() { base.MultiWriteTest(); }
        [Fact]public override void NonTrackedUpdateTest() { base.NonTrackedUpdateTest(); }
        [Fact]public override void GetMaxAsync() { base.GetMaxAsync(); }
    }

    public class TestEntityQueryableMemory: TestEntityQueryableBase
    {
        public TestEntityQueryableMemory()
        {
            queryable = CreateService<EntityQueryableMemory>();
        }

        [Fact] public override void SimpleReadTest() { base.SimpleReadTest(); }
        [Fact] public override void SimpleUpdateTest() { base.SimpleUpdateTest(); }
        [Fact] public override void SimpleWriteTest() { base.SimpleWriteTest(); }
        [Fact] public override void SimpleDeleteTest() { base.SimpleDeleteTest(); }
        [Fact] public override void MultiWriteTest() { base.MultiWriteTest(); }
        [Fact]public override void NonTrackedUpdateTest() { base.NonTrackedUpdateTest(); }
        [Fact]public override void GetMaxAsync() { base.GetMaxAsync(); }
    }
}