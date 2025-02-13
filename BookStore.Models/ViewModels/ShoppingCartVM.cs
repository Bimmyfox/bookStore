using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using BookStore.Models;

namespace BookStore.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public OrderHeader OrderHeader { get; set; }
        public required IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
    }
}