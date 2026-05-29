using Microsoft.AspNetCore.Identity;

namespace CustomerRequestPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public string? StaffPosition { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
