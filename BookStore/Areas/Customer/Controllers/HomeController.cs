using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BookStore.Models;
using BookStore.DataAccess.Repository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookStore.Utility;

namespace BookStore.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly IUnitOfWork unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        this.logger = logger;
        this.unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        if(claim != null)
        {
            HttpContext.Session.SetInt32(SD.SESSION_CART, 
                unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == claim.Value).Count());
        }
        
        IEnumerable<Product> products = unitOfWork.ProductRepository.GetAll(includeProperties: "Category,ProductImages");
        return View(products);
    }

    public IActionResult Details(int productId)
    {
        ShoppingCart shoppingCart = new()
        {
            Product = unitOfWork.ProductRepository.Get(u => u.Id == productId, includeProperties: "Category,ProductImages"),
            Count = 1,
            ProductId = productId
        };
        return View(shoppingCart);
    }

    [HttpPost]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        shoppingCart.ApplicationUserId = userId;

        ShoppingCart cartDb = unitOfWork.ShoppingCartRepository.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

        if(cartDb != null)
        {
            cartDb.Count += shoppingCart.Count;
            unitOfWork.ShoppingCartRepository.Update(cartDb);
            unitOfWork.Save();
        }
        else
        {
            unitOfWork.ShoppingCartRepository.Add(shoppingCart);
            unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.SESSION_CART, 
                unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId).Count());
        }
        TempData["success"] = "Cart updated successfully";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
