using BookStore.DataAccess.Data;
using BookStore.Models;


namespace BookStore.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext db;


        public ApplicationUserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            db = dbContext;
        }

        public void Update(ApplicationUser applicationUser) {
            db.ApplicationUsers.Update(applicationUser);
        }
    }
}