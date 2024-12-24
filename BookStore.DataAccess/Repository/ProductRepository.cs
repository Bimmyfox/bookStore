using BookStore.DataAccess.Data;
using BookStore.Models;


namespace BookStore.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext db;


        public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            db = dbContext;
        }

        public void Update(Product product)
        {
            db.Products.Update(product);
        }
    }
}