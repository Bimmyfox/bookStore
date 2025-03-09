using BookStore.DataAccess.Repository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using BookStore.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Stripe;
using Stripe.BillingPortal;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BookStore.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    [Authorize(Roles = SD.ROLE_ADMIN)]
    public class UserController : Controller
    {
        // private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IUnitOfWork unitOfWork;

        public UserController(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager) {
            this.unitOfWork = unitOfWork;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region APICalls
        [HttpGet]
       [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = unitOfWork.ApplicationUserRepository.GetAll(includeProperties: "Company").ToList();

            foreach(var user in objUserList) {

                user.Role = userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

                if (user.Company == null) {
                    user.Company = new Company() {
                        Name = ""
                    };
                }
            }

            return Json(new { data = objUserList });
        }


       public IActionResult RoleManagement(string userId) {

            RoleManagementVM RoleVM = new RoleManagementVM() {
                ApplicationUser = unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId, includeProperties:"Company"),
                RoleList = roleManager.Roles.Select(i => new SelectListItem {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = unitOfWork.CompanyRepository.GetAll().Select(i => new SelectListItem {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            RoleVM.ApplicationUser.Role = userManager.GetRolesAsync(unitOfWork.ApplicationUserRepository.Get(u=>u.Id==userId))
                    .GetAwaiter().GetResult().FirstOrDefault();
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM) {

            string oldRole  = userManager.GetRolesAsync(unitOfWork.ApplicationUserRepository.Get(u => u.Id == roleManagementVM.ApplicationUser.Id))
                    .GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = unitOfWork.ApplicationUserRepository.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);


            if (!(roleManagementVM.ApplicationUser.Role == oldRole)) {
                //a role was updated
                if (roleManagementVM.ApplicationUser.Role == SD.ROLE_COMPANY) {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if (oldRole == SD.ROLE_COMPANY) {
                    applicationUser.CompanyId = null;
                }
                unitOfWork.ApplicationUserRepository.Update(applicationUser);
                unitOfWork.Save();

                userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();

            }
            else {
                if(oldRole == SD.ROLE_COMPANY && applicationUser.CompanyId != roleManagementVM.ApplicationUser.CompanyId) {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                    unitOfWork.ApplicationUserRepository.Update(applicationUser);
                    unitOfWork.Save();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {

            var objFromDb = unitOfWork.ApplicationUserRepository.Get(u => u.Id == id);
            if (objFromDb == null) 
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if(objFromDb.LockoutEnd!=null && objFromDb.LockoutEnd > DateTime.Now) {
                //user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            unitOfWork.ApplicationUserRepository.Update(objFromDb);
            unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }
        #endregion
    }
}