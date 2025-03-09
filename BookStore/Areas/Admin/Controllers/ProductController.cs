using BookStore.DataAccess.Repository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    [Authorize(Roles = SD.ROLE_ADMIN)]
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
            List<Product> objProductList = unitOfWork.ProductRepository.GetAll(includeProperties: "Category").ToList();
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
                productVM.Product = unitOfWork.ProductRepository.Get(u => u.Id == id, includeProperties:"ProductImages");
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {
            if(ModelState.IsValid)
            {
                if(productVM.Product.Id == 0)
                {
                    unitOfWork.ProductRepository.Add(productVM.Product);
                }
                else
                {
                    unitOfWork.ProductRepository.Update(productVM.Product);
                }
                unitOfWork.Save();

                string wwwRootPath = webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    foreach(IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images/products/product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if(!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageUrl = @"/" + productPath + @"/" + fileName,
                            ProductId = productVM.Product.Id
                        };

                        if(productVM.Product.ProductImages == null)
                        {
                            productVM.Product.ProductImages = new List <ProductImage>();
                        }

                        productVM.Product.ProductImages.Add(productImage);
                    }

                    unitOfWork.ProductRepository.Update(productVM.Product);
                    unitOfWork.Save();
                }

                TempData["success"]="Product created/update successfully";
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

        public IActionResult DeleteImage(int? imageId)
        {
            var imageToBeDeleted = unitOfWork.ProductImageRepository.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if(imageToBeDeleted != null)
            {
                if(!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImgPath = 
                        Path.Combine(webHostEnvironment.WebRootPath, 
                        imageToBeDeleted.ImageUrl.TrimStart('/'));
                    if(System.IO.File.Exists(oldImgPath))
                    {
                        System.IO.File.Delete(oldImgPath);
                    }
                }

                unitOfWork.ProductImageRepository.Remove(imageToBeDeleted);
                unitOfWork.Save();

                TempData["success"]="Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new {id = productId});
        }

        #region APICalls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = unitOfWork.ProductRepository.GetAll(includeProperties: "Category").ToList();
            return Json(new {data = objProductList });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = unitOfWork.ProductRepository.Get(u => u.Id == id);
            if(productToBeDeleted == null)
            {
                return Json(new { success = false, message="Error. Product was not found"});
            }

            string productPath = @"images/products/product-" + id;
            string finalPath = Path.Combine(webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach(string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }

            unitOfWork.ProductRepository.Remove(productToBeDeleted);
            unitOfWork.Save();
            return Json(new { success = true, message="Product was deleted"});
        }
        #endregion
    }
}