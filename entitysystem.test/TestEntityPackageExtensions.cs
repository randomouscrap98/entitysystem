using Xunit;
using System;
using Randomous.EntitySystem.Extensions;

namespace Randomous.EntitySystem.test
{
    public class EntityPackageExtensionsTest : UnitTestBase //ServiceTestBase<IEntityProvider>
    {
        public EntityPackage QuickEntity(string name = "name")
        {
            return new EntityPackage()
            {
                Entity = new Entity()
                {
                    name = name
                }
            };
        }

        public EntityValue QuickValue(string key, string value)
        {
            return new EntityValue()
            {
                key = key, value = value
            };
        }

        public EntityRelation QuickRelation(long entity1, long entity2, string type, string value = null)
        {
            return new EntityRelation()
            {
                createDate = DateTime.Now,
                entityId1 = entity1,
                entityId2 = entity2,
                type = type,
                value = value
            };
        }

        //[Fact]
        //public void QuickEntity()
        //{
        //    var entity = EntityPackageExtensions.QuickEntity("someName", "someContent");
        //    Assert.Equal("someName", entity.name);
        //    Assert.Equal("someContent", entity.content);
        //    Assert.True((DateTime.Now - entity.createDate).TotalSeconds < 5);
        //}

        //[Fact]
        //public void AddValue()
        //{
        //    var entity = QuickEntity("aname")
        //        .Add(QuickValue("key1", "value1"))
        //        .Add(QuickValue("key2", "value2"));
        //    
        //    Assert.Empty(entity.Relations);
        //    Assert.Equal(2, entity.Values.Count);
        //    Assert.Equal("key1", entity.Values[0].key);
        //    Assert.Equal("value2", entity.Values[1].value);
        //}

        //[Fact]
        //public void AddRelation()
        //{
        //    var entity = EntityWrapperExtensions.QuickEntity("names")
        //        .AddRelation(1, "yes")
        //        .AddRelation(3, "no", "value");

        //    Assert.Empty(entity.Values);
        //    Assert.Equal(2, entity.Relations.Count);
        //    Assert.Equal(1, entity.Relations[0].entityId1);
        //    Assert.Equal(entity.id, entity.Relations[1].entityId2);
        //    Assert.Equal("yes", entity.Relations[0].type);
        //    Assert.Equal("value", entity.Relations[1].value);
        //}

        protected EntityPackage GetBasicWrapper()
        {
            return QuickEntity("aname")
                .Add(QuickValue("key1", "value1"))
                .Add(QuickValue("key2", "value2"))
                .Add(QuickRelation(1, 2, "yes"))
                .Add(QuickRelation(3, 4, "no", "value"));
        }

        [Fact]
        public void HasValue()
        {
            var entity = GetBasicWrapper();
            Assert.True(entity.HasValue("key1"));
            Assert.False(entity.HasValue("yes"));
        }

        [Fact]
        public void GetValue()
        {
            var entity = GetBasicWrapper();
            Assert.Equal("value2", entity.GetValue("key2").value);
            Assert.ThrowsAny<InvalidOperationException>(() => entity.GetValue("no"));
        }

        [Fact]
        public void SetEntityNew()
        {
            var entity = GetBasicWrapper();
            entity.Entity.id = 5;
            entity.Values.ForEach(x => x.id = 99);
            entity.Relations.ForEach(x => x.id = 45);

            var other = entity.NewCopy();

            //A new entity shoudl have no ids. there may be more things but this is all we're testing right now
            Assert.Equal(0, other.Entity.id);
            Assert.All(other.Values, x => Assert.Equal(0, x.id));
            Assert.All(other.Relations, x => Assert.Equal(0, x.id));
        }
    }
}