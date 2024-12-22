using BookStore.DataAccess.Repository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Category> objCategoryList = unitOfWork.CategoryRepository.GetAll().ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if(category.Name == "test")
            {
                ModelState.AddModelError("", "Invalid value - test");
            }
            if(ModelState.IsValid)
            {
                unitOfWork.CategoryRepository.Add(category);
                unitOfWork.Save();
                TempData["success"]="Category created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }


        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            Category? category =  unitOfWork.CategoryRepository.Get(u => u.Id == id);
            // var category2 =  unitOfWork.CategoryRepository.Categories.FirstOrDefault(u => u.Id == id);
            // var category3 =  unitOfWork.CategoryRepository.Categories.Where(u => u.Id == id).FirstOrDefault();
            if(category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if(ModelState.IsValid)
            {
                 unitOfWork.CategoryRepository.Update(category);
                 unitOfWork.Save();
                TempData["success"]="Category edited successfully";
                return RedirectToAction("Index");
            }
            return View();
        }


         public IActionResult Delete(int? id)
        {
            if(id == null || id == 0) return NotFound();

            Category? category =  unitOfWork.CategoryRepository.Get(u => u.Id == id);
            if(category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            if(id == null || id == 0) return NotFound();

            Category? category =  unitOfWork.CategoryRepository.Get(u => u.Id == id);
            if(category == null)
            {
                return NotFound();
            }
             unitOfWork.CategoryRepository.Remove(category);
             unitOfWork.Save();
            TempData["success"]="Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}