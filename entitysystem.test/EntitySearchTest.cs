using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace entitysystem.test
{
    public class EntitySearchTest : UnitTestBase
    {
        public EntitySearcher CreateSearcher()
        {
            return CreateService<EntitySearcher>();
        }

        public List<Entity> BasicEntityList(int count = 100)
        {
            var entities = new List<Entity>();

            for(int i = 0; i < count; i++)
            {
                entities.Add(new Entity()
                {
                    id = i + 1,
                    createDate = DateTime.Now.AddDays(-i)
                });
            }

            return entities;
        }

        [Fact]
        public void TestEmptySearch()
        {
            var entities = BasicEntityList();
            var searcher = CreateSearcher();

            //An empty search should return all entities
            Assert.Equal(searcher.ApplyGeneric<Entity>(entities.AsQueryable(), new EntitySearch()).Count(), entities.Count);
        }

        [Fact]
        public void TestSearchIds()
        {
            var entities = BasicEntityList(50);
            var searcher = CreateSearcher();
            var search = new EntitySearch();

            Assert.True(entities.Count == 50);

            //Try searching for 1 or more ids
            search.Ids.Add(1);
            var result = searcher.ApplyGeneric<Entity>(entities.AsQueryable(), search).ToList();
            Assert.Single(result);
            Assert.Equal(result, entities.Where(x => x.id == 1).ToList());

            search.Ids.Add(20);
            result = searcher.ApplyGeneric<Entity>(entities.AsQueryable(), search).ToList();
            Assert.Equal(2, result.Count);
            Assert.Equal(result, entities.Where(x => x.id == 1 || x.id == 20).ToList());
        }
    }
}