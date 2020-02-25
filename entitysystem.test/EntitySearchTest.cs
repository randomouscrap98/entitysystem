using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace entitysystem.test
{
    public class EntitySearchTest : UnitTestBase
    {
        public IEntitySearcher CreateSearcher()
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
            Assert.Equal(searcher.ApplyEntitySearch(entities.AsQueryable(), new EntitySearch()).Count(), entities.Count);
        }
    }
}