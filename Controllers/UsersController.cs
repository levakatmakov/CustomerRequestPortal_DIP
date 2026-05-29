using CustomerRequestPortal.Models;
using CustomerRequestPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerRequestPortal.Controllers
{
    [Authorize(Roles = AppRoles.Administrator)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToList();

            return View(users);
        }

        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            var allowedRoles = new[] { AppRoles.Dispatcher, AppRoles.Executor, AppRoles.Administrator };
            if (!allowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Эту роль может назначить только администратор.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Role = model.Role,
                StaffPosition = model.Role == AppRoles.Executor ? model.StaffPosition : AppRoles.GetDisplayName(model.Role),
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                TempData["Success"] = $"Пользователь \"{user.FullName}\" создан с ролью {AppRoles.GetDisplayName(model.Role)}.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Пользователь не найден.";
                return RedirectToAction(nameof(Index));
            }

            if (user.Id == currentUser.Id)
            {
                TempData["Error"] = "Нельзя удалить собственную учетную запись.";
                return RedirectToAction(nameof(Index));
            }

            if (user.Role == AppRoles.Administrator)
            {
                var administrators = await _userManager.GetUsersInRoleAsync(AppRoles.Administrator);
                if (administrators.Count <= 1)
                {
                    TempData["Error"] = "Нельзя удалить последнего администратора системы.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"Пользователь \"{user.FullName}\" удален.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }
    }
}
