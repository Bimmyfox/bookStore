using System.Security.Claims;
using BookStore.DataAccess.Repository;
using BookStore.Utility;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
       private readonly IUnitOfWork unitOfWork;

       public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
       {
            this.unitOfWork = unitOfWork;
       }

       public async Task<IViewComponentResult> InvokeAsync()
       {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if(claim != null)
            {
                if (HttpContext.Session.GetInt32(SD.SESSION_CART) == null)
                {
                    HttpContext.Session.SetInt32(SD.SESSION_CART, 
                        unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == claim.Value).Count());
                } 
                return View(HttpContext.Session.GetInt32(SD.SESSION_CART));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }

       }
    }
}