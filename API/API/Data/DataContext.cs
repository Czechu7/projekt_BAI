using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) 
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<FileEntity> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
        .Property(u => u.Id)
        .ValueGeneratedOnAdd();

          
        }
    }
}
