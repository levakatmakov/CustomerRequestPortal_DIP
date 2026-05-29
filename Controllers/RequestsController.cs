using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerRequestPortal.Models;
using CustomerRequestPortal.Data;

namespace CustomerRequestPortal.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: Requests
        public async Task<IActionResult> Index(string? search, string? status, string? priority, string sort = "date_desc")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            IQueryable<CustomerRequest> requests;

            if (user.Role == AppRoles.Customer)
            {
                requests = _db.CustomerRequests
                    .Include(r => r.AssignedExecutor)
                    .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                    .Where(r => r.UserId == user.Id);
            }
            else if (user.Role == AppRoles.Executor)
            {
                requests = _db.CustomerRequests
                    .Include(r => r.User)
                    .Include(r => r.AssignedExecutor)
                    .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                    .Where(r => r.AssignedExecutorId == user.Id);
            }
            else
            {
                requests = _db.CustomerRequests
                    .Include(r => r.User)
                    .Include(r => r.AssignedExecutor)
                    .Include(r => r.Items)
                    .ThenInclude(i => i.Product);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchText = search.Trim();
                requests = requests.Where(r =>
                    r.RequestNumber.Contains(searchText) ||
                    r.Title.Contains(searchText) ||
                    (r.Description != null && r.Description.Contains(searchText)) ||
                    (r.User != null && r.User.FullName.Contains(searchText)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r => r.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                requests = requests.Where(r => r.Priority == priority);
            }

            requests = sort switch
            {
                "date_asc" => requests.OrderBy(r => r.CreatedAt),
                "amount_desc" => requests.OrderByDescending(r => r.TotalAmount),
                "amount_asc" => requests.OrderBy(r => r.TotalAmount),
                "priority" => requests
                    .OrderByDescending(r => r.Priority == "Высокий")
                    .ThenByDescending(r => r.Priority == "Средний")
                    .ThenByDescending(r => r.CreatedAt),
                _ => requests.OrderByDescending(r => r.CreatedAt)
            };

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Priority = priority;
            ViewBag.Sort = sort;
            ViewBag.TotalRequests = await requests.CountAsync();
            ViewBag.NewRequests = await requests.CountAsync(r => r.Status == "Новая");
            ViewBag.AssignedRequests = await requests.CountAsync(r => r.Status == "Назначена");
            ViewBag.ReadyRequests = await requests.CountAsync(r => r.Status == "Готов к отгрузке");
            ViewBag.CompletedRequests = await requests.CountAsync(r => r.Status == "Завершена");

            return View(await requests.ToListAsync());
        }

        // GET: Requests/Create
        [Authorize(Roles = "Customer")]
        public IActionResult Create()
        {
            ViewBag.Products = _db.Products.Where(p => p.IsActive).ToList();
            return View();
        }

        // POST: Requests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(CustomerRequest request, int[] productIds, int[] quantities)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Пользователь не найден. Пожалуйста, войдите в систему.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Проверка товаров
                if (productIds == null || quantities == null || productIds.Length == 0)
                {
                    ViewBag.Products = _db.Products.Where(p => p.IsActive).ToList();
                    TempData["Error"] = "Добавьте хотя бы один товар в заявку.";
                    return View(request);
                }

                // Проверяем доступность товаров
                for (int i = 0; i < productIds.Length; i++)
                {
                    if (productIds[i] > 0 && quantities[i] > 0)
                    {
                        var product = await _db.Products.FindAsync(productIds[i]);
                        if (product == null)
                        {
                            ViewBag.Products = _db.Products.Where(p => p.IsActive).ToList();
                            TempData["Error"] = $"Товар не найден!";
                            return View(request);
                        }

                        if (product.StockQuantity < quantities[i])
                        {
                            ViewBag.Products = _db.Products.Where(p => p.IsActive).ToList();
                            TempData["Error"] = $"Недостаточно товара \"{product.Name}\" на складе. Доступно: {product.StockQuantity} шт.";
                            return View(request);
                        }
                    }
                }

                // Создаём заявку
                request.UserId = user.Id;
                request.RequestNumber = $"REQ-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
                request.Status = "Новая";
                request.CreatedAt = DateTime.Now;
                request.TotalAmount = 0;

                _db.CustomerRequests.Add(request);
                await _db.SaveChangesAsync();

                _db.RequestStatusHistories.Add(new RequestStatusHistory
                {
                    RequestId = request.Id,
                    OldStatus = string.Empty,
                    NewStatus = request.Status,
                    Comment = "Заявка создана клиентом",
                    ChangedByUserId = user.Id,
                    ChangedAt = DateTime.Now
                });

                // Добавляем товары и уменьшаем остаток
                decimal total = 0;
                for (int i = 0; i < productIds.Length; i++)
                {
                    if (productIds[i] > 0 && quantities[i] > 0)
                    {
                        var product = await _db.Products.FindAsync(productIds[i]);
                        if (product != null)
                        {
                            _db.RequestItems.Add(new RequestItem
                            {
                                RequestId = request.Id,
                                ProductId = product.Id,
                                Quantity = quantities[i],
                                Price = product.Price
                            });

                            total += product.Price * quantities[i];

                            // УМЕНЬШАЕМ остаток на складе
                            product.StockQuantity -= quantities[i];
                            _db.Products.Update(product);
                        }
                    }
                }

                request.TotalAmount = total;
                await _db.SaveChangesAsync();

                TempData["Success"] = $"✅ Ваша заявка {request.RequestNumber} успешно создана! Сумма: {total:N2} ₽";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Products = _db.Products.Where(p => p.IsActive).ToList();
                TempData["Error"] = $"❌ Ошибка при создании заявки: {ex.Message}";
                return View(request);
            }
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var request = await _db.CustomerRequests
                .Include(r => r.User)
                .Include(r => r.AssignedExecutor)
                .Include(r => r.Items)
                .ThenInclude(i => i.Product)
                .Include(r => r.StatusHistory!)
                .ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!CanViewRequest(user, request))
                return Forbid();

            ViewBag.Executors = await _userManager.GetUsersInRoleAsync(AppRoles.Executor);
            return View(request);
        }

        // GET: Requests/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var request = await _db.CustomerRequests
                .Include(r => r.User)
                .Include(r => r.AssignedExecutor)
                .Include(r => r.Items)
                .ThenInclude(i => i.Product)
                .Include(r => r.StatusHistory)
                .ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!CanViewRequest(user, request))
                return Forbid();

            return View(request);
        }

        // POST: Requests/AssignExecutor
        [Authorize(Roles = $"{AppRoles.Dispatcher},{AppRoles.Administrator}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignExecutor(int id, string executorId, DateTime? dueDate, string? comment)
        {
            var request = await _db.CustomerRequests.FindAsync(id);
            if (request != null)
            {
                var user = await _userManager.GetUserAsync(User);
                var oldStatus = request.Status;

                if (request.Status != "Новая" || !string.IsNullOrEmpty(request.AssignedExecutorId))
                {
                    TempData["Error"] = "Заявка уже назначена. Повторное назначение исполнителя недоступно.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var executor = await _userManager.FindByIdAsync(executorId);
                if (executor == null || executor.Role != AppRoles.Executor)
                {
                    TempData["Error"] = "Выберите корректного исполнителя.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                request.AssignedExecutorId = executor.Id;
                request.DueDate = dueDate;
                request.Status = "Назначена";
                request.ManagerComment = comment;
                request.CompletedAt = null;

                _db.RequestStatusHistories.Add(new RequestStatusHistory
                {
                    RequestId = request.Id,
                    OldStatus = oldStatus,
                    NewStatus = request.Status,
                    Comment = $"Назначен исполнитель: {executor.FullName}. {comment}",
                    ChangedByUserId = user?.Id,
                    ChangedAt = DateTime.Now
                });

                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Requests/UpdateExecutorStatus
        [Authorize(Roles = AppRoles.Executor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateExecutorStatus(int id, string status, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var allowedStatuses = new[] { "Подтверждено наличие", "Комплектуется", "Готов к отгрузке" };
            if (!allowedStatuses.Contains(status))
            {
                return BadRequest();
            }

            var request = await _db.CustomerRequests.FindAsync(id);
            if (request == null) return NotFound();
            if (request.AssignedExecutorId != user.Id) return Forbid();

            if (request.Status == "Готов к отгрузке" || request.Status == "Завершена")
            {
                TempData["Error"] = "После готовности к отгрузке исполнитель больше не может менять статус заявки.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var oldStatus = request.Status;
            request.Status = status;
            request.ManagerComment = comment;

            request.CompletedAt = null;

            _db.RequestStatusHistories.Add(new RequestStatusHistory
            {
                RequestId = request.Id,
                OldStatus = oldStatus,
                NewStatus = status,
                Comment = comment,
                ChangedByUserId = user.Id,
                ChangedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Requests/CompletePickup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletePickup(int id, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var request = await _db.CustomerRequests.FindAsync(id);
            if (request == null) return NotFound();

            var canComplete = (user.Role == AppRoles.Customer && request.UserId == user.Id) ||
                              user.Role == AppRoles.Dispatcher ||
                              user.Role == AppRoles.Administrator;

            if (!canComplete) return Forbid();

            if (request.Status != "Готов к отгрузке")
            {
                TempData["Error"] = "Завершить можно только заявку со статусом \"Готов к отгрузке\".";
                return RedirectToAction(nameof(Details), new { id });
            }

            var oldStatus = request.Status;
            request.Status = "Завершена";
            request.CompletedAt = DateTime.Now;
            request.ManagerComment = comment;

            _db.RequestStatusHistories.Add(new RequestStatusHistory
            {
                RequestId = request.Id,
                OldStatus = oldStatus,
                NewStatus = request.Status,
                Comment = string.IsNullOrWhiteSpace(comment) ? "Клиент забрал заказ" : comment,
                ChangedByUserId = user.Id,
                ChangedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Requests/Delete
        [Authorize(Roles = AppRoles.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _db.CustomerRequests.FindAsync(id);
            if (request != null)
            {
                _db.CustomerRequests.Remove(request);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private static bool CanViewRequest(ApplicationUser user, CustomerRequest request)
        {
            return user.Role switch
            {
                AppRoles.Customer => request.UserId == user.Id,
                AppRoles.Executor => request.AssignedExecutorId == user.Id,
                AppRoles.Dispatcher => true,
                AppRoles.Administrator => true,
                _ => false
            };
        }
    }
}
