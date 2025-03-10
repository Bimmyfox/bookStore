using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookStore.Models
{
    public class Product
    {
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
        
        [Key]
        public int Id { get; set; }
        
        [Required] 
        public string Title { get; set; }
        [Required] 
        public string Author { get; set; }
        [Required] 
        public string ISBN { get; set; }
        [Required] [DisplayName("List Price")] [Range(1, 1000)]
        public double ListPrice { get; set; }

        [Required] [DisplayName("Price for 1-50")] [Range(1, 1000)]
        public double Price { get; set; }

        [Required] [DisplayName("Price for 50+")] [Range(1, 1000)]
        public double Price50 { get; set; }

        [Required] [DisplayName("Price for 100+")] [Range(1, 1000)]
        public double Price100 { get; set; }

        public string Description { get; set; }

        [ValidateNever]
        public int CategoryId { get; set; }

        [ValidateNever]
        public List<ProductImage> ProductImages { get; set; }
    }
}