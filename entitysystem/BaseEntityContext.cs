using Microsoft.EntityFrameworkCore;

namespace entitysystem
{
    public class BaseEntityContext : DbContext
    {
        public BaseEntityContext(DbContextOptions<BaseEntityContext> options) : base(options) {}

        public DbSet<Entity> Entities {get;set;}
        public DbSet<EntityValue> EntityValues {get;set;}
        public DbSet<EntityRelation> EntityRelations {get;set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>()
                .HasKey(x => x.id)
                ;

            modelBuilder.Entity<EntityValue>()
                .HasKey(x => x.id)
                ;

            modelBuilder.Entity<EntityRelation>()
                .HasKey(x => x.id)
                ;
        }
    }
}