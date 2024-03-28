using Microsoft.EntityFrameworkCore;
using PlusNine.Entities.DbSet;

namespace PlusNine.DataService.Data
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<Objective> Objectives { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

        }
    }
}
