using Xunit;

namespace entitysystem.test
{
    public class EntityProviderEfCoreTest: UnitTestBase
    {
        protected EntityProviderEfCore provider;

        public EntityProviderEfCoreTest()
        {
            provider = CreateService<EntityProviderEfCore>();
            provider.context.Database.EnsureCreated();  
        }

        public Entity NewSingleEntity()
        {
            return new Entity()
            {
                type = "Yeah",
                name = "The ENtity",
                content = "A lot of content comes from here!"
            };
        }

        [Fact]
        public void SimpleProviderWriteTest()
        {
            //Can we insert objects and get them out?
            provider.WriteEntities(new[] {NewSingleEntity()}).Wait();
        }
    }
}