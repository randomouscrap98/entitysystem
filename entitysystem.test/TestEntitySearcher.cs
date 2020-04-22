using System;
using System.Collections.Generic;
using System.Linq;
using Randomous.EntitySystem.Implementations;
using Xunit;

namespace Randomous.EntitySystem.test
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
    public class TestEntitySearcher : UnitTestBase
    {
        protected IEntitySearcher searcher;

        public TestEntitySearcher()
        {
            searcher = CreateService<EntitySearcher>();
        }

        public List<E> GenerateBaseData<E>(int count = 100) where E : EntityBase, new()
        {
            var entities = new List<E>();

            for(int i = 0; i < count; i++)
            {
                var e = new E()
                {
                    id = i + 1,
                    createDate = DateTime.Now.AddDays(-i)
                };
                if(e is EntityRelation)
                {
                    var er = (EntityRelation)(object)e;
                    er.entityId1 = count - i;
                    er.entityId2 = count - i;
                }
                if(e is EntityValue)
                {
                    var er = (EntityValue)(object)e;
                    er.entityId = count - i;
                }
                entities.Add(e);
            }

            return entities;
        }

        public virtual IQueryable<E> BasicDataset<E>(int count = 100) where E : EntityBase, new()
        {
            return GenerateBaseData<E>().AsQueryable();
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

        protected void SimpleIdsTest<E,S>(Func<S, IQueryable<E>, IQueryable<E>> applySearch, Func<S, IList<long>> searchIdGet, Func<E, long> fieldGet) where E : EntityBase, new () where S : EntitySearchBase, new()
        {
            var entities = BasicDataset<E>();
            var search = new S();

            //Try searching for 1 or more ids
            searchIdGet(search).Add(1);
            var result = applySearch(search, entities);
            AssertResultsEqual(entities.Where(x => fieldGet(x) == 1), result);

            searchIdGet(search).Add(20);
            result = applySearch(search, entities);
            AssertResultsEqual(entities.Where(x => fieldGet(x)== 1 || fieldGet(x) == 20), result);

            searchIdGet(search).Add(1); //It SHOULD BE acceptable to have duplicates
            result = applySearch(search, entities);
            AssertResultsEqual(entities.Where(x => fieldGet(x) == 1 || fieldGet(x) == 20), result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SearchBaseIds() { 
            SimpleIdsTest<EntityBase, EntitySearchBase>((s, e) => searcher.ApplyGeneric<EntityBase>(e, s), s => s.Ids, e => e.id); }

        [Fact]
        public void SearchEntityIds() { 
            SimpleIdsTest<Entity, EntitySearch>((s, e) => searcher.ApplyEntitySearch(e, s), s => s.Ids, e => e.id); }

        [Fact]
        public void SearchEntityValueIds() { 
            SimpleIdsTest<EntityValue, EntityValueSearch>((s, e) => searcher.ApplyEntityValueSearch(e, s), s => s.Ids, e => e.id); }

        [Fact]
        public void SearchEntityRelationIds() { 
            SimpleIdsTest<EntityRelation, EntityRelationSearch>((s, e) => searcher.ApplyEntityRelationSearch(e, s), s => s.Ids, e => e.id); }


        [Fact]
        public void SearchRelationEntity1() { 
            SimpleIdsTest<EntityRelation, EntityRelationSearch>((s, e) => searcher.ApplyEntityRelationSearch(e, s), s => s.EntityIds1, e => e.entityId1); }

        [Fact]
        public void SearchRelationEntity2() { 
            SimpleIdsTest<EntityRelation, EntityRelationSearch>((s, e) => searcher.ApplyEntityRelationSearch(e, s), s => s.EntityIds2, e => e.entityId2); }

        [Fact]
        public void SearchEntityValueEntity() { 
            SimpleIdsTest<EntityValue, EntityValueSearch>((s, e) => searcher.ApplyEntityValueSearch(e, s), s => s.EntityIds, e => e.entityId); }

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

        protected Tuple<string, IQueryable<E>> SetupStringLikeTest<E>(Action<E, string> setField, IQueryable<E> entities) where E : EntityBase
        {
            var set = new Action<long, string>((i, s) => setField(entities.First(x => x.id == i), s));

            //Set some to have some names/types
            set(4, "one");
            set(6, "ones");
            set(8, "lone");
            set(10, "lones");
            set(12, "lons");

            //Just for fun
            for(int i = 11; i < 20; i++)
                set(i, "");

            return Tuple.Create("%one%", entities.Where(x => x.id == 4 || x.id == 6 || x.id == 8 || x.id == 10));
        }

        protected void SimpleStringLikeTest<E>(Action<E, string> setField, Func<string, IQueryable<E>, IQueryable<E>> doSearch) where E : EntityBase, new()
        {
            var entities = BasicDataset<E>();
            var setup = SetupStringLikeTest(setField, entities);
            var result = doSearch(setup.Item1, entities);
            AssertResultsEqual(setup.Item2, result);
        }

        [Fact]
        public void SearchEntityType()
        {
            SimpleStringLikeTest<Entity>((e, s) => e.type = s, (s, e) => {
                return searcher.ApplyEntitySearch(e, new EntitySearch() {TypeLike = s});
            });
        }

        [Fact]
        public void SearchEntityName()
        {
            SimpleStringLikeTest<Entity>((e, s) => e.name = s, (s, e) => {
                return searcher.ApplyEntitySearch(e, new EntitySearch() {NameLike = s});
            });
        }

        [Fact]
        public void SearchRelationType()
        {
            SimpleStringLikeTest<EntityRelation>((e, s) => e.type = s, (s, e) => {
                return searcher.ApplyEntityRelationSearch(e, new EntityRelationSearch() {TypeLike= s});
            });
        }

        [Fact]
        public void SearchValueKey()
        {
            SimpleStringLikeTest<EntityValue>((e, s) => e.key = s, (s, e) => {
                return searcher.ApplyEntityValueSearch(e, new EntityValueSearch() {KeyLike = s});
            });
        }

        [Fact]
        public void SearchValueValue()
        {
            SimpleStringLikeTest<EntityValue>((e, s) => e.value = s, (s, e) => {
                return searcher.ApplyEntityValueSearch(e, new EntityValueSearch() {ValueLike = s});
            });
        }

        protected void SimpleSkipLimitTest<E,S>(Func<S, IQueryable<E>, IQueryable<E>> applySearch) where E : EntityBase, new () where S : EntitySearchBase, new()
        {
            var entities = BasicDataset<E>();
            var search = new S();

            search.Limit = 10;
            var result = applySearch(search, entities);
            Assert.Equal(10, result.Count());

            int counter = 1;
            foreach(var e in result)
                Assert.Equal(counter++, e.id);

            search.Skip = 20;
            result = applySearch(search, entities);
            Assert.Equal(10, result.Count());

            counter = 21;
            foreach(var e in result)
                Assert.Equal(counter++, e.id);

            search.Reverse = true;
            result = applySearch(search, entities);
            Assert.Equal(10, result.Count());

            counter = entities.Count() - 20;
            foreach(var e in result)
                Assert.Equal(counter--, e.id);
        }

        [Fact]
        public void SearchBaseSkipLimit() { 
            SimpleSkipLimitTest<EntityBase, EntitySearchBase>((s, e) => searcher.ApplyFinal<EntityBase>(e, s)); }

        [Fact]
        public void SearchEntitySkipLimit() { 
            SimpleSkipLimitTest<Entity, EntitySearch>((s, e) => searcher.ApplyEntitySearch(e, s)); }

        [Fact]
        public void SearchEntityValueSKipLimit() { 
            SimpleSkipLimitTest<EntityValue, EntityValueSearch>((s, e) => searcher.ApplyEntityValueSearch(e, s)); }

        [Fact]
        public void SearchEntityRelationSKipLimit() { 
            SimpleSkipLimitTest<EntityRelation, EntityRelationSearch>((s, e) => searcher.ApplyEntityRelationSearch(e, s)); }

        protected void SimpleMaxMinTest<E,S>(Func<S, IQueryable<E>, IQueryable<E>> applySearch) where E : EntityBase, new() where S : EntitySearchBase, new()
        {
            var entities = BasicDataset<E>();
            var search = new S();

            search.MaxId = 20;//CreateStart = DateTime.Now.AddDays(-10);
            var result = applySearch(search, entities);
            Assert.True(result.All(x => x.id < 20));

            search.MinId = 10;
            result = applySearch(search, entities);
            Assert.True(result.All(x => x.id < 20 && x.id > 10));
        }

        [Fact]
        public void SearchBaseMaxMin() { SimpleMaxMinTest<EntityBase, EntitySearchBase>((s, e) => searcher.ApplyGeneric<EntityBase>(e, s)); }

        [Fact]
        public void SearchEntityMaxMin() { SimpleMaxMinTest<Entity, EntitySearch>((s, e) => searcher.ApplyEntitySearch(e, s)); }

        [Fact]
        public void SearchEntityValueMaxMin() { SimpleMaxMinTest<EntityValue, EntityValueSearch>((s, e) => searcher.ApplyEntityValueSearch(e, s)); }

        [Fact]
        public void SearchEntityRelationMaxMin() { SimpleMaxMinTest<EntityRelation, EntityRelationSearch>((s, e) => searcher.ApplyEntityRelationSearch(e, s)); }

        protected void SimpleSortTest<E,S>(Func<S, IQueryable<E>, IQueryable<E>> applySearch) where E : EntityBase, new () where S : EntitySearchBase, new()
        {
            var entities = BasicDataset<E>();
            var search = new S();

            search.Sort = "random";
            var result = applySearch(search, entities);
            var result2 = applySearch(search, entities);

            Assert.False(result.Select(x => x.id).SequenceEqual(result2.Select(x => x.id)));
        }

        [Fact]
        public void SearchBaseSort() { SimpleSortTest<EntityBase, EntitySearchBase>((s, e) => searcher.ApplyGeneric<EntityBase>(e, s)); }

        [Fact]
        public void SearchEntitySort() { SimpleSortTest<Entity, EntitySearch>((s, e) => searcher.ApplyEntitySearch(e, s)); }

        [Fact]
        public void SearchEntityValueSort() { SimpleSortTest<EntityValue, EntityValueSearch>((s, e) => searcher.ApplyEntityValueSearch(e, s)); }

        [Fact]
        public void SearchEntityRelationSort() { SimpleSortTest<EntityRelation, EntityRelationSearch>((s, e) => searcher.ApplyEntityRelationSearch(e, s)); }
    }

    //public class TestEntitySearchLive : TestEntitySearcher
    //{
    //    protected List<IEntityProvider> providers = new List<IEntityProvider>();

    //    public override IQueryable<E> BasicDataset<E>(int count = 100)
    //    {
    //        //Create some NEW provider EVERY TIME wow
    //        var provider = CreateService<IEntityProvider>();
    //        var entities = GenerateBaseData<E>(count);
    //        provider.WriteAsync(entities.ToArray());
    //        providers.Add(provider); //I don't know why I'm keeping track of this
    //        return provider.GetQueryable<E>();
    //    }
    //}
}