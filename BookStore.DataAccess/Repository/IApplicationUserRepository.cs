using BookStore.Models;

namespace BookStore.DataAccess.Repository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        public void Update(ApplicationUser applicationUser);
    }
}