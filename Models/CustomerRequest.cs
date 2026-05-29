using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerRequestPortal.Models
{
    public class CustomerRequest
    {
        public int Id { get; set; }

        public required string UserId { get; set; }
        [ForeignKey("UserId")]
        public required ApplicationUser User { get; set; }

        public required string RequestNumber { get; set; }
        public required string Title { get; set; }

        public string? Description { get; set; }
        public string Status { get; set; } = "Новая";
        public string Priority { get; set; } = "Средний";

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ManagerComment { get; set; }

        public string? AssignedExecutorId { get; set; }
        [ForeignKey("AssignedExecutorId")]
        public ApplicationUser? AssignedExecutor { get; set; }

        public ICollection<RequestItem> Items { get; set; } = new List<RequestItem>();
        public ICollection<RequestStatusHistory> StatusHistory { get; set; } = new List<RequestStatusHistory>();
    }
}
