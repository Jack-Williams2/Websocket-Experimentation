using Microsoft.EntityFrameworkCore;
using WebsocketApi.Model.Persistance;
namespace WebsocketApi.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Sample> samples { get; set; }
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your entities here
            base.OnModelCreating(modelBuilder);
        }
        // Define DbSets for your entities
        // public DbSet<YourEntity> YourEntities { get; set; }
    }
}
