using BookStore.DataAccess.Repository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            ProductVM productVM = new()
            {
                CategoryList = unitOfWork.CategoryRepository.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Create(ProductVM productVM)
        {
            if(ModelState.IsValid)
            {
                unitOfWork.ProductRepository.Add(productVM.Product);
                unitOfWork.Save();
                TempData["success"]="Product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = unitOfWork.CategoryRepository.GetAll()
                    .Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    });
                return View(productVM);
            }
        }


        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            Product? product =  unitOfWork.ProductRepository.Get(u => u.Id == id);
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