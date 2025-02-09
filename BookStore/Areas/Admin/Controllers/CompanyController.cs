using BookStore.DataAccess.Repository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    // [Authorize(Roles = SD.ROLE_ADMIN)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> objCompanyList = unitOfWork.CompanyRepository.GetAll().ToList();
            return View(objCompanyList);
        }

        public IActionResult Upsert(int? id)
        {
            if(id == null || id == 0)
            {
                //create
                return View(new Company());
            }
            else
            {
                //update
                Company companyObj = unitOfWork.CompanyRepository.Get(u => u.Id == id);
                return View(companyObj);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if(ModelState.IsValid)
            {
                if(company.Id == 0)
                {
                    unitOfWork.CompanyRepository.Add(company);
                }
                else
                {
                    unitOfWork.CompanyRepository.Update(company);
                }
                unitOfWork.Save();
                TempData["success"]="Company created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(company);
            }
        }


        #region APICalls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = unitOfWork.CompanyRepository.GetAll().ToList();
            return Json(new {data = objCompanyList });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var company = unitOfWork.CompanyRepository.Get(u => u.Id == id);
            if(company == null)
            {
                return Json(new { success = false, message="company was not found"});
            }

            unitOfWork.CompanyRepository.Remove(company);
            unitOfWork.Save();
            return Json(new { success = true, message="company was deleted"});
        }
        #endregion
    }
}