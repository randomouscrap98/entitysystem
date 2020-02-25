using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace entitysystem.test
{
    public class EntitySearchTest : UnitTestBase
    {
        protected EntitySearcher searcher;

        public EntitySearchTest()
        {
            searcher = CreateService<EntitySearcher>();
        }

        public IQueryable<E> BasicDataset<E>(int count = 100) where E : EntityBase, new()
        {
            var entities = new List<E>();

            for(int i = 0; i < count; i++)
            {
                entities.Add(new E()
                {
                    id = i + 1,
                    createDate = DateTime.Now.AddDays(-i)
                });
            }

            return entities.AsQueryable();
        }

        [Fact]
        public void TestEmptySearch()
        {
            var entities = BasicDataset<EntityBase>();

            //An empty search should return all entities
            var result = searcher.ApplyGeneric<EntityBase>(entities, new EntitySearch());
            AssertResultsEqual(result, entities);
        }

        [Fact]
        public void TestSearchIds()
        {
            var entities = BasicDataset<EntityBase>();
            var search = new EntitySearch();

            //Try searching for 1 or more ids
            search.Ids.Add(1);
            var result = searcher.ApplyGeneric<EntityBase>(entities, search);
            AssertResultsEqual(entities.Where(x => x.id == 1), result);

            search.Ids.Add(20);
            result = searcher.ApplyGeneric<EntityBase>(entities, search);
            AssertResultsEqual(entities.Where(x => x.id == 1 || x.id == 20), result);

            search.Ids.Add(1); //It SHOULD BE acceptable to have duplicates
            result = searcher.ApplyGeneric<EntityBase>(entities, search);
            AssertResultsEqual(entities.Where(x => x.id == 1 || x.id == 20), result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void TestSearchDates()
        {
            var entities = BasicDataset<EntityBase>();
            var search = new EntitySearch();

            search.CreateStart = DateTime.Now.AddDays(-10);
            var result = searcher.ApplyGeneric<EntityBase>(entities, search);
            Assert.True(result.Count() >= 9 && result.Count() <= 11);
            AssertResultsEqual(entities.Where(x => x.createDate >= search.CreateStart), result);

            search.CreateEnd = DateTime.Now.AddDays(-5);
            result = searcher.ApplyGeneric<EntityBase>(entities, search);
            Assert.True(result.Count() >= 4 && result.Count() <= 6);
            AssertResultsEqual(entities.Where(x => x.createDate >= search.CreateStart && x.createDate <= search.CreateEnd), result);
        }

        protected Tuple<string, IQueryable<E>> SetupRegexTest<E>(Action<E, string> setField, IQueryable<E> entities) where E : EntityBase
        {
            var set = new Action<long, string>((i, s) => setField(entities.First(x => x.id == i), s));

            //Set some to have some names/types
            set(4, "one");
            set(6, "One");
            set(8, "oNE");
            set(10, "ones");

            //Just for fun
            for(int i = 11; i < 20; i++)
                set(i, "");

            return Tuple.Create("^(?i:one)$", entities.Where(x => x.id == 4 || x.id == 6 || x.id == 8));
        }

        protected void SimpleRegexTest<E>(Action<E, string> setField, Func<string, IQueryable<E>, IQueryable<E>> doSearch) where E : EntityBase, new()
        {
            var entities = BasicDataset<E>();
            var setup = SetupRegexTest(setField, entities);
            var result = doSearch(setup.Item1, entities);
            AssertResultsEqual(setup.Item2, result);
        }

        [Fact]
        public void TestSearchEntityType()
        {
            SimpleRegexTest<Entity>((e, s) => e.type = s, (s, e) =>
            {
                return searcher.ApplyEntitySearch(e, new EntitySearch() {TypeRegex = s});
            });
        }

        [Fact]
        public void TestSearchEntityName()
        {
            SimpleRegexTest<Entity>((e, s) => e.name = s, (s, e) =>
            {
                return searcher.ApplyEntitySearch(e, new EntitySearch() {NameRegex= s});
            });
        }
    }
}