using Microsoft.EntityFrameworkCore;

namespace Randomous.EntitySystem
{
    /// <summary>
    /// A basic context that describes the defaults for tables and relationships in the entity system.
    /// </summary>
    public class BaseEntityContext : DbContext
    {
        public BaseEntityContext(DbContextOptions<BaseEntityContext> options) : base(options) {}

        public DbSet<Entity> Entities {get;set;}
        public DbSet<EntityValue> EntityValues {get;set;}
        public DbSet<EntityRelation> EntityRelations {get;set;}

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Note: this isn't supposed to create a PERFECT, all complete table. This is the 
        /// bare minimum to get this to work.
        /// </remarks>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>()
                .HasKey(x => x.id);

            modelBuilder.Entity<EntityValue>()
                .HasKey(x => x.id);

            modelBuilder.Entity<EntityRelation>()
                .HasKey(x => x.id);
        }
    }
}