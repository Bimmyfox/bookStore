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
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = unitOfWork.ProductRepository.GetAll().ToList();
            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
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
            if(id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = unitOfWork.ProductRepository.Get(u => u.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if(ModelState.IsValid)
            {
                string wwwRootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images/product");

                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //delete old img
                        var oldImgPath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('/'));
                        if(System.IO.File.Exists(oldImgPath))
                        {
                            System.IO.File.Delete(oldImgPath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"/images/product/" + fileName;
                }

                if(productVM.Product.Id == 0)
                {
                    unitOfWork.ProductRepository.Add(productVM.Product);
                }
                else
                {
                    unitOfWork.ProductRepository.Update(productVM.Product);
                }
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