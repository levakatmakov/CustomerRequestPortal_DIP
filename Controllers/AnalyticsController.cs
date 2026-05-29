using CustomerRequestPortal.Data;
using CustomerRequestPortal.Models;
using CustomerRequestPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerRequestPortal.Controllers
{
    [Authorize(Roles = $"{AppRoles.Dispatcher},{AppRoles.Administrator}")]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AnalyticsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var sixMonthsAgo = monthStart.AddMonths(-5);

            var requests = await _db.CustomerRequests.ToListAsync();
            var recentRequests = requests.Where(r => r.CreatedAt >= sixMonthsAgo).ToList();
            var totalRequests = requests.Count;

            var model = new AnalyticsViewModel
            {
                TotalRequests = totalRequests,
                NewRequests = requests.Count(r => r.Status == "Новая"),
                InWorkRequests = requests.Count(r => r.Status == "Назначена" || r.Status == "Подтверждено наличие" || r.Status == "Комплектуется"),
                DoneRequests = requests.Count(r => r.Status == "Завершена"),
                RejectedRequests = requests.Count(r => r.Status == "Отклонена"),
                TotalRevenue = requests.Sum(r => r.TotalAmount),
                MonthRevenue = requests.Where(r => r.CreatedAt >= monthStart).Sum(r => r.TotalAmount),
                ActiveProducts = await _db.Products.CountAsync(p => p.IsActive),
                LowStockProductsCount = await _db.Products.CountAsync(p => p.StockQuantity <= 100),
                LowStockProducts = await _db.Products
                    .Where(p => p.StockQuantity <= 100)
                    .OrderBy(p => p.StockQuantity)
                    .Take(8)
                    .ToListAsync()
            };

            model.StatusSummary = new[] { "Новая", "Назначена", "Подтверждено наличие", "Комплектуется", "Готов к отгрузке", "Завершена", "Отклонена" }
                .Select(status =>
                {
                    var count = requests.Count(r => r.Status == status);
                    return new StatusSummaryItem
                    {
                        Status = status,
                        Count = count,
                        Percent = totalRequests == 0 ? 0 : Math.Round(count * 100m / totalRequests, 1)
                    };
                })
                .ToList();

            model.PopularProducts = await _db.RequestItems
                .Include(i => i.Product)
                .GroupBy(i => new { i.ProductId, ProductName = i.Product != null ? i.Product.Name : "Товар удалён" })
                .Select(g => new PopularProductItem
                {
                    ProductName = g.Key.ProductName,
                    Quantity = g.Sum(i => i.Quantity),
                    Amount = g.Sum(i => i.Price * i.Quantity)
                })
                .OrderByDescending(i => i.Quantity)
                .Take(6)
                .ToListAsync();

            model.MonthlyRevenue = Enumerable.Range(0, 6)
                .Select(offset => sixMonthsAgo.AddMonths(offset))
                .Select(month =>
                {
                    var monthRequests = recentRequests
                        .Where(r => r.CreatedAt.Year == month.Year && r.CreatedAt.Month == month.Month)
                        .ToList();

                    return new MonthlyRevenueItem
                    {
                        Month = month.ToString("MM.yyyy"),
                        Amount = monthRequests.Sum(r => r.TotalAmount),
                        RequestsCount = monthRequests.Count
                    };
                })
                .ToList();

            return View(model);
        }
    }
}
