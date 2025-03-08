using BookStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BookStore.DataAccess.Data;
using BookStore.Utility;

namespace BookStore.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.db = db;
        }
        

        public void Initialize()
        {
            db.Database.EnsureCreated();
            try
            {
                if(db.Database.GetPendingMigrations().Count() > 0)
                {
                    db.Database.Migrate();
                }
            }
            catch(Exception ex) {}

            if(!roleManager.RoleExistsAsync(SD.ROLE_CUSTOMER).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(SD.ROLE_CUSTOMER)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.ROLE_COMPANY)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.ROLE_ADMIN)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.ROLE_EMPLOYEE)).GetAwaiter().GetResult();

                userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@test.com",
                    Email = "admin@test.com",
                    Name = "Daria Test",
                    PhoneNumber = "88005553535",
                    StreetAddress = "test street 33",
                    State = "At",
                    PostalCode = "22333",
                    City = "test"
                }, "qq11Q!").GetAwaiter().GetResult();

                ApplicationUser user = db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@test.com");
                userManager.AddToRoleAsync(user, SD.ROLE_ADMIN).GetAwaiter().GetResult();

            }
            return;
        }

    }
}