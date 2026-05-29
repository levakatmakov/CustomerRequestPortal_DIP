using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerRequestPortal.Models
{
    public class RequestItem
    {
        public int Id { get; set; }

        public int RequestId { get; set; }
        [ForeignKey("RequestId")]
        public CustomerRequest? Request { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total => Price * Quantity;
    }
}