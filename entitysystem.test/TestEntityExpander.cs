//using System;
//using Randomous.EntitySystem.Implementations;
//
//namespace Randomous.EntitySystem.test
//{
//    public class TestEntityExpander : UnitTestBase
//    {
//        protected EntityExpander expander;
//
//        public TestEntityExpander()
//        {
//            expander = CreateService<EntityExpander>();
//        }
//
//        public Entity NewEntity()
//        {
//            return new Entity()
//            {
//                createDate = DateTime.Now,
//                type = "Yeah",
//                name = "The ENtity",
//                content = "A lot of content comes from here!"
//            };
//        }
//
//        public EntityValue NewValue()
//        {
//            return new EntityValue()
//            {
//                createDate = DateTime.Now,
//                key = "someKey",
//                value = "some value"
//            };
//        }
//
//        public EntityRelation NewRelation()
//        {
//            return new EntityRelation()
//            {
//                createDate = DateTime.Now,
//                entityId1 = 5, //Just some value; foreign key constraints shouldn't be present I think!!
//                type = "someType",
//                value = "some relation value"
//            };
//        }
//
//        public EntityPackage NewPackage()
//        {
//            var package = new EntityPackage() { Entity = NewEntity()};
//            var value = NewValue();
//            var relation = NewRelation();
//            expander.AddValues(package, value);
//            expander.AddRelations(package, relation);
//            return package;
//        }
//
//        
//    }
//}