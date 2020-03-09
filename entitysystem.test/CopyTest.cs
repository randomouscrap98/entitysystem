//using System;
//using System.Linq;
//using Xunit;
//
//namespace Randomous.EntitySystem.test
//{
//    public class CopyTest
//    {
//        public EntitySearchBase GetBasic()
//        {
//            var baseSearch = new EntitySearchBase();
//            baseSearch.CreateStart = DateTime.Now.AddDays(5);
//            baseSearch.CreateEnd = DateTime.Now;
//            baseSearch.Limit = 50;
//            baseSearch.Skip = 30;
//            baseSearch.Reverse = true;
//            baseSearch.Ids.Add(5);
//            baseSearch.Ids.Add(7);
//            return baseSearch;
//        }
//
//        protected void BasicAssert(EntitySearchBase baseSearch, EntitySearchBase second)
//        {
//            Assert.Equal(baseSearch.CreateStart, second.CreateStart);
//            Assert.Equal(baseSearch.CreateEnd, second.CreateEnd);
//            Assert.Equal(baseSearch.Limit, second.Limit);
//            Assert.Equal(baseSearch.Skip, second.Skip);
//            Assert.Equal(baseSearch.Reverse, second.Reverse);
//            Assert.True(baseSearch.Ids.SequenceEqual(second.Ids));
//        }
//
//        protected void BasicTest<T>(Func<EntitySearchBase, T> create) where T : EntitySearchBase
//        {
//            var baseSearch = GetBasic();
//            var second = create(baseSearch);
//            BasicAssert(baseSearch, second);
//        }
//
//        [Fact]
//        public void CopyBaseSearch() { BasicTest((f) => new EntitySearchBase(f)); }
//
//        [Fact]
//        public void CopyEntitySearch() { BasicTest((f) => new EntitySearch(f)); }
//
//        [Fact]
//        public void CopyEntityValueSearch() { BasicTest((f) => new EntityValueSearch(f)); }
//
//        [Fact]
//        public void CopyEntityRelationSearch() { BasicTest((f) => new EntityRelationSearch(f)); }
//    }
//}