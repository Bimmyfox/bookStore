using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BookStore.Models;
using BookStore.DataAccess.Repository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookStore.Areas.Customer.Controllers;

[Area("Customer")]
public class CartController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}