using BookStore.DataAccess.Data;
using BookStore.Models;


namespace BookStore.DataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private ApplicationDbContext db;


        public ProductImageRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            db = dbContext;
        }

        public void Update(ProductImage productImage)
        {
            db.ProductImages.Update(productImage);
        }
    }
}