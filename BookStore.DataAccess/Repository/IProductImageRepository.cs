using BookStore.Models;

namespace BookStore.DataAccess.Repository
{
    public interface IProductImageRepository : IRepository<ProductImage>
    {
        void Update(ProductImage productImage);
    }
}