using CustomerRequestPortal.Data;
using CustomerRequestPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerRequestPortal.Controllers
{
    [Authorize(Roles = AppRoles.Customer)]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var notifications = await _db.RequestStatusHistories
                .Include(h => h.Request)
                .Include(h => h.ChangedByUser)
                .Where(h => h.Request != null && h.Request.UserId == user.Id)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            return View(notifications);
        }
    }
}
