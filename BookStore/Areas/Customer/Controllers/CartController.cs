using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BookStore.Models.ViewModels;
using BookStore.DataAccess.Repository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookStore.Models;
using BookStore.Utility;
using Stripe;
using Stripe.BillingPortal;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BookStore.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{

    private readonly IUnitOfWork unitOfWork;
    private readonly IEmailSender emailSender;

    [BindProperty]
    public ShoppingCartVM ShoppingCartVM { get; set; }

    public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
    {
        this.unitOfWork = unitOfWork;
        this.emailSender = emailSender;
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
        var cartDb = unitOfWork.ShoppingCartRepository.Get(u => u.Id == cartId, tracked: true);
        if (cartDb.Count <= 1)
        {
            HttpContext.Session.SetInt32(SD.SESSION_CART, 
                unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == cartDb.ApplicationUserId).Count() - 1);
       
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

    public IActionResult Remove(int cartId)
    {
        var cartDb = unitOfWork.ShoppingCartRepository.Get(u => u.Id == cartId, tracked: true);
        HttpContext.Session.SetInt32(SD.SESSION_CART, 
            unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == cartDb.ApplicationUserId).Count() - 1);
        unitOfWork.ShoppingCartRepository.Remove(cartDb);
        unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Summary()
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

        ShoppingCartVM.OrderHeader.ApplicationUser = unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

        ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
        ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
        ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
        ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
        ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
        ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
      
        
        foreach(var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }
        return View(ShoppingCartVM);
    }

    [HttpPost]
    [ActionName("Summary")]
    public IActionResult SummaryPOST()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVM.ShoppingCartList = unitOfWork.ShoppingCartRepository.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product");

        ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
        ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
                
        ApplicationUser applicationUser = unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);
 
        foreach(var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        if(applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PAYMENT_STATUS_PENDING;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.STATUS_PENDING;
        }
        else 
        {
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PAYMENT_STATUS_DELAYED_PAYMENT;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.STATUS_APPROVED;
        }

        unitOfWork.OrderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
        unitOfWork.Save();

        foreach(var cart in ShoppingCartVM.ShoppingCartList)
        {
            OrderDetail orderDetail = new ()
            {
                ProductId = cart.ProductId,
                OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count
            };
            unitOfWork.OrderDetailRepository.Add(orderDetail);
            unitOfWork.Save();
        }

        if(applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach(var item in ShoppingCartVM.ShoppingCartList)
            {
                var sessionLineItem = new Stripe.Checkout.SessionLineItemOptions
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }

        return RedirectToAction(nameof(OrderConfirmation), 
                                new { id = ShoppingCartVM.OrderHeader.Id});
    }

    public IActionResult OrderConfirmation(int id)
    {
        OrderHeader orderHeader = unitOfWork.OrderHeaderRepository.Get(u => u.Id == id, includeProperties: "ApplicationUser");
        if(orderHeader.PaymentStatus != SD.PAYMENT_STATUS_DELAYED_PAYMENT)
        {
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Get(orderHeader.SessionId);

            if(session.PaymentStatus.ToLower() == "paid")
            {
                unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                unitOfWork.OrderHeaderRepository.UpdateStatus(id, SD.STATUS_APPROVED, SD.PAYMENT_STATUS_APPROVED);
                unitOfWork.Save();
            }
            HttpContext.Session.Clear();
        }

        emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Book Store",
            $"<p>New Order Created - {orderHeader.Id}</p>");

        List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCartRepository
                                    .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

        unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCarts);
        unitOfWork.Save();

        return View(id);
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