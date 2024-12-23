using BookStore.DataAccess.Repository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = unitOfWork.ProductRepository.GetAll().ToList();
            return View(objProductList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            if(product.Title == "test")
            {
                ModelState.AddModelError("", "Invalid value - test");
            }
            if(ModelState.IsValid)
            {
                unitOfWork.ProductRepository.Add(product);
                unitOfWork.Save();
                TempData["success"]="Product created successfully";
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
            Product? product =  unitOfWork.ProductRepository.Get(u => u.Id == id);
            // var product2 =  unitOfWork.ProductRepository.Categories.FirstOrDefault(u => u.Id == id);
            // var product3 =  unitOfWork.ProductRepository.Categories.Where(u => u.Id == id).FirstOrDefault();
            if(product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if(ModelState.IsValid)
            {
                 unitOfWork.ProductRepository.Update(product);
                 unitOfWork.Save();
                TempData["success"]="Product edited successfully";
                return RedirectToAction("Index");
            }
            return View();
        }


         public IActionResult Delete(int? id)
        {
            if(id == null || id == 0) return NotFound();

            Product? product =  unitOfWork.ProductRepository.Get(u => u.Id == id);
            if(product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            if(id == null || id == 0) return NotFound();

            Product? product =  unitOfWork.ProductRepository.Get(u => u.Id == id);
            if(product == null)
            {
                return NotFound();
            }
             unitOfWork.ProductRepository.Remove(product);
             unitOfWork.Save();
            TempData["success"]="Product deleted successfully";
            return RedirectToAction("Index");
        }
    }
}