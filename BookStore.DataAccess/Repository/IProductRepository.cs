using BookStore.Models;

namespace BookStore.DataAccess.Repository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
    }
}