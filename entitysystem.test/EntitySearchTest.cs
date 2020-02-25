using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace entitysystem.test
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This system is set up with a broad scope in mind. There are many generic bits that attempt to simplify
    /// running standard tests on every type of entity object. Although the entity system is a VAST simplification
    /// over most normal systems, there are still duplication issues across the three entity types. This is to 
    /// keep the database from becoming TOO generic and simplify use; with builtin types, names, content etc, the
    /// system may very well have what you need out of the box.
    /// </remarks>
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

        protected void SimpleEmptyTest<E,S>(Func<S, IQueryable<E>, IQueryable<E>> applySearch) where E : EntityBase, new() where S : EntitySearchBase, new()
        {
            var entities = BasicDataset<E>();
            var result = applySearch(new S(), entities);
            AssertResultsEqual(entities, result);
        }

        [Fact]
        public void SearchBaseEmpty() { SimpleEmptyTest<EntityBase, EntitySearchBase>((s, e) => searcher.ApplyGeneric<EntityBase>(e, s)); }

        [Fact]
        public void SearchEntityEmpty() { SimpleEmptyTest<Entity, EntitySearch>((s, e) => searcher.ApplyEntitySearch(e, s)); }

        [Fact]
        public void SearchEntityValueEmpty() { SimpleEmptyTest<EntityValue, EntityValueSearch>((s, e) => searcher.ApplyEntityValueSearch(e, s)); }

        [Fact]
        public void SearchEntityRelationEmpty() { SimpleEmptyTest<EntityRelation, EntityRelationSearch>((s, e) => searcher.ApplyEntityRelationSearch(e, s)); }

        protected void SimpleIdsTest<E,S>(Func<S, IQueryable<E>, IQueryable<E>> applySearch) where E : EntityBase, new() where S : EntitySearchBase, new()
        {
            var entities = BasicDataset<E>();
            var search = new S();

            //Try searching for 1 or more ids
            search.Ids.Add(1);
            var result = applySearch(search, entities);
            AssertResultsEqual(entities.Where(x => x.id == 1), result);

            search.Ids.Add(20);
            result = applySearch(search, entities);
            AssertResultsEqual(entities.Where(x => x.id == 1 || x.id == 20), result);

            search.Ids.Add(1); //It SHOULD BE acceptable to have duplicates
            result = applySearch(search, entities);
            AssertResultsEqual(entities.Where(x => x.id == 1 || x.id == 20), result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SearchBaseIds() { SimpleIdsTest<EntityBase, EntitySearchBase>((s, e) => searcher.ApplyGeneric<EntityBase>(e, s)); }

        [Fact]
        public void SearchEntityIds() { SimpleIdsTest<Entity, EntitySearch>((s, e) => searcher.ApplyEntitySearch(e, s)); }

        [Fact]
        public void SearchEntityValueIds() { SimpleIdsTest<EntityValue, EntityValueSearch>((s, e) => searcher.ApplyEntityValueSearch(e, s)); }

        [Fact]
        public void SearchEntityRelationIds() { SimpleIdsTest<EntityRelation, EntityRelationSearch>((s, e) => searcher.ApplyEntityRelationSearch(e, s)); }

        protected void SimpleDatesTest<E,S>(Func<S, IQueryable<E>, IQueryable<E>> applySearch) where E : EntityBase, new() where S : EntitySearchBase, new()
        {
            var entities = BasicDataset<E>();
            var search = new S();

            search.CreateStart = DateTime.Now.AddDays(-10);
            var result = applySearch(search, entities);
            Assert.True(result.Count() >= 9 && result.Count() <= 11);
            AssertResultsEqual(entities.Where(x => x.createDate >= search.CreateStart), result);

            search.CreateEnd = DateTime.Now.AddDays(-5);
            result = applySearch(search, entities);
            Assert.True(result.Count() >= 4 && result.Count() <= 6);
            AssertResultsEqual(entities.Where(x => x.createDate >= search.CreateStart && x.createDate <= search.CreateEnd), result);
        }

        [Fact]
        public void SearchBaseDates() { SimpleDatesTest<EntityBase, EntitySearchBase>((s, e) => searcher.ApplyGeneric<EntityBase>(e, s)); }

        [Fact]
        public void SearchEntityDates() { SimpleDatesTest<Entity, EntitySearch>((s, e) => searcher.ApplyEntitySearch(e, s)); }

        [Fact]
        public void SearchEntityValueDates() { SimpleDatesTest<EntityValue, EntityValueSearch>((s, e) => searcher.ApplyEntityValueSearch(e, s)); }

        [Fact]
        public void SearchEntityRelationDates() { SimpleDatesTest<EntityRelation, EntityRelationSearch>((s, e) => searcher.ApplyEntityRelationSearch(e, s)); }

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
        public void SearchEntityType()
        {
            SimpleRegexTest<Entity>((e, s) => e.type = s, (s, e) => {
                return searcher.ApplyEntitySearch(e, new EntitySearch() {TypeRegex = s});
            });
        }

        [Fact]
        public void SearchEntityName()
        {
            SimpleRegexTest<Entity>((e, s) => e.name = s, (s, e) => {
                return searcher.ApplyEntitySearch(e, new EntitySearch() {NameRegex= s});
            });
        }

        [Fact]
        public void SearchRelationType()
        {
            SimpleRegexTest<EntityRelation>((e, s) => e.type = s, (s, e) => {
                return searcher.ApplyEntityRelationSearch(e, new EntityRelationSearch() {TypeRegex = s});
            });
        }

        [Fact]
        public void SearchValueKey()
        {
            SimpleRegexTest<EntityValue>((e, s) => e.key = s, (s, e) => {
                return searcher.ApplyEntityValueSearch(e, new EntityValueSearch() {KeyRegex = s});
            });
        }

        [Fact]
        public void SearchValueValue()
        {
            SimpleRegexTest<EntityValue>((e, s) => e.value = s, (s, e) => {
                return searcher.ApplyEntityValueSearch(e, new EntityValueSearch() {ValueRegex= s});
            });
        }
    }
}