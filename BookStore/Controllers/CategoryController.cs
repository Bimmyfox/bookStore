using BookStore.DataAccess.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BookStore.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext db;

        public CategoryController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            List<Category> objCategoryList = db.Categories.ToList();
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
                db.Categories.Add(category);
                db.SaveChanges();
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
            Category? category = db.Categories.Find(id);
            // var category2 = db.Categories.FirstOrDefault(u => u.Id == id);
            // var category3 = db.Categories.Where(u => u.Id == id).FirstOrDefault();
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
                db.Categories.Update(category);
                db.SaveChanges();
                TempData["success"]="Category edited successfully";
                return RedirectToAction("Index");
            }
            return View();
        }


         public IActionResult Delete(int? id)
        {
            if(id == null || id == 0) return NotFound();

            Category? category = db.Categories.Find(id);
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

            Category? category = db.Categories.Find(id);
            if(category == null)
            {
                return NotFound();
            }
            db.Categories.Remove(category);
            db.SaveChanges();
            TempData["success"]="Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}