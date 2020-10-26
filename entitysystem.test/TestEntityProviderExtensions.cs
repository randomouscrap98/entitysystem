using System;
using System.Linq;
using Randomous.EntitySystem.Extensions;
using Randomous.EntitySystem.Implementations;
using Xunit;

namespace Randomous.EntitySystem.test
{
    public class TestEntityProviderExtensions : UnitTestBase
    {
        public EntityProvider provider; 

        public TestEntityProviderExtensions()
        {
            provider = CreateService<EntityProvider>();
        }

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
            package.Add(value);
            package.Add(relation);
            return package;
        }

        //WARN: CAN'T TRACK SAME THING FROM MULTIPLE PLACES!!! How will
        //multithreading work?? 

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

            var qresult = provider.GetQueryableAsync<Entity>().Result;
            var result = provider.GetListAsync(qresult).Result;
            Assert.Single(result);
            Assert.Equal(package.Entity, result.First());

            var expanded = provider.LinkAsync(qresult).Result;
            Assert.Single(expanded);
            Assert.Equal(package, expanded.First());
        }

        [Fact]
        public virtual void EntityPackageExpandRewrite()
        {
            //This creates a package with 1 value and 1 relation
            var package = NewPackage();
            provider.WriteAsync(package).Wait();

            var qresult = provider.GetQueryableAsync<Entity>().Result;
            var result = provider.GetListAsync(qresult).Result;
            var expanded = provider.LinkAsync(qresult).Result.First();
            expanded.Values.First().value = "lolButts";
            provider.WriteAsync(expanded).Wait();

            qresult = provider.GetQueryableAsync<Entity>().Result;
            result = provider.GetListAsync(qresult).Result;
            var expanded2 = provider.LinkAsync(qresult).Result;
            Assert.Single(expanded2);
            Assert.Equal(expanded, expanded2.First());
        }

        [Fact]
        public virtual void EntityPackageBiggerExpand()
        {
            //This creates a package with 1 value and 1 relation
            var package = NewPackage();
            package.Add(NewValue()).Add(NewValue()).Add(NewValue());
            package.Add(NewRelation()).Add(NewRelation()).Add(NewRelation());
            provider.WriteAsync(package).Wait();
            Assert.True(package.Entity.id > 0);

            var qresult = provider.GetQueryableAsync<Entity>().Result;
            var result = provider.GetListAsync(qresult).Result;
            Assert.Single(result);
            Assert.Equal(package.Entity, result.First());

            var expanded = provider.LinkAsync(qresult).Result;
            Assert.Single(expanded);
            Assert.Equal(package, expanded.First());
        }
    }
}