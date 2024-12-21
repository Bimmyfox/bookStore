using BookStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        public DbSet<Category> Categories { get; set; }

        public ApplicationDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to postgres with connection string from app settings
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category {Id = 1, Name="Action", DisplayOrder = 1 },
                new Category {Id = 2, Name="Documentary", DisplayOrder = 2 },
                new Category {Id = 3, Name="Science", DisplayOrder = 3 }
            );
        }
    }   
}