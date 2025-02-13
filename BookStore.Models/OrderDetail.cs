using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookStore.Models
{
    public class OrderDetail
    {
        [ForeignKey("OrderHeaderId")]
        [ValidateNever]
        
        public OrderHeader OrderHeader { get; set; }
        
        [ForeignKey("ProductId")]
        [ValidateNever]
        
        public Product Product { get; set; }

        [Required] 
        public int OrderHeaderId { get; set; }

        [Required] 
        public int ProductId { get; set; }

        public int Id { get; set; }
        public int Count { get; set; }
        public double Price { get; set; }

    }
}