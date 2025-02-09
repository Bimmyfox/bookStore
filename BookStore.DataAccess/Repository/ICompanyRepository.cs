using BookStore.Models;

namespace BookStore.DataAccess.Repository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company company);
    }
}