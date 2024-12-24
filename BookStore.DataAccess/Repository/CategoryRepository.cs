using BookStore.DataAccess.Data;
using BookStore.Models;


namespace BookStore.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext db;


        public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            db = dbContext;
        }

        public void Update(Category category)
        {
            db.Categories.Update(category);
        }
    }
}