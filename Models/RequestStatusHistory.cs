using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerRequestPortal.Models
{
    public class RequestStatusHistory
    {
        public int Id { get; set; }

        public int RequestId { get; set; }
        [ForeignKey("RequestId")]
        public CustomerRequest? Request { get; set; }

        [Required]
        public string OldStatus { get; set; } = string.Empty;

        [Required]
        public string NewStatus { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public string? ChangedByUserId { get; set; }
        [ForeignKey("ChangedByUserId")]
        public ApplicationUser? ChangedByUser { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}
