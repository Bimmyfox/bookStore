using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using BookStore.Models;

namespace BookStore.Models.ViewModels
{
    public class ProductVM
    {
        public required Product Product { get; set; }
        [ValidateNever]
        public required IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}