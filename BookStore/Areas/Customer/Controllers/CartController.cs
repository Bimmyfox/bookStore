using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BookStore.Models.ViewModels;
using BookStore.DataAccess.Repository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookStore.Models;

namespace BookStore.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{

    private readonly IUnitOfWork unitOfWork;
    public ShoppingCartVM ShoppingCartVM { get; set; }

    public CartController(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }


    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVM = new()
        {
            ShoppingCartList = unitOfWork.ShoppingCartRepository.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
        };
        
        foreach(var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }
        return View(ShoppingCartVM);
    }

    public IActionResult Plus(int cartId)
    {
        var cartDb = unitOfWork.ShoppingCartRepository.Get(u => u.Id == cartId);
        cartDb.Count++;
        unitOfWork.ShoppingCartRepository.Update(cartDb);
        unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Minus(int cartId)
    {
        var cartDb = unitOfWork.ShoppingCartRepository.Get(u => u.Id == cartId);
        if (cartDb.Count <= 1)
        {
            unitOfWork.ShoppingCartRepository.Remove(cartDb);
        }
        else
        {
            cartDb.Count--;
            unitOfWork.ShoppingCartRepository.Update(cartDb);
        }
        unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Summary()
    {
        return View();
    }

    public IActionResult Remove(int cartId)
    {
        var cartDb = unitOfWork.ShoppingCartRepository.Get(u => u.Id == cartId);
        unitOfWork.ShoppingCartRepository.Remove(cartDb);
        unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }
    private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
    {
        if(shoppingCart.Count <= 50)
        {
            return shoppingCart.Product.Price;
        }
        else if(shoppingCart.Count <= 100)
        {
            return shoppingCart.Product.Price50;
        }
        else
        {
            return shoppingCart.Product.Price100;
        }
    }
}