using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerRequestPortal.Data;
using CustomerRequestPortal.Models;

namespace CustomerRequestPortal.Controllers
{
    [Authorize(Roles = AppRoles.Administrator)]
    public class DiagnosticsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DiagnosticsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Check()
        {
            var info = new System.Text.StringBuilder();

            info.AppendLine("<!DOCTYPE html><html><head><title>Диагностика</title>");
            info.AppendLine("<style>body{font-family:Arial;padding:20px;} h2{color:#2c5282;} h3{color:#3182ce;margin-top:20px;} ul{list-style:none;padding:0;} li{padding:5px;margin:5px 0;background:#f7fafc;border-radius:5px;}</style>");
            info.AppendLine("</head><body>");
            info.AppendLine("<h2>🔧 Диагностика системы</h2>");

            // Проверка таблиц
            info.AppendLine("<h3>📊 Таблицы базы данных:</h3><ul>");
            info.AppendLine($"<li>📋 Заявок: <strong>{await _db.CustomerRequests.CountAsync()}</strong></li>");
            info.AppendLine($"<li>📦 Товаров: <strong>{await _db.Products.CountAsync()}</strong></li>");
            info.AppendLine($"<li>🛒 Позиций в заявках: <strong>{await _db.RequestItems.CountAsync()}</strong></li>");
            info.AppendLine("</ul>");

            // Последние заявки
            info.AppendLine("<h3>📋 Последние заявки:</h3><ul>");
            var lastRequests = await _db.CustomerRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            if (lastRequests.Any())
            {
                foreach (var req in lastRequests)
                {
                    info.AppendLine($"<li><strong>{req.RequestNumber}</strong> - {req.Title} | Статус: {req.Status} | Сумма: {req.TotalAmount:N2} ₽ | Клиент: {req.User?.FullName}</li>");
                }
            }
            else
            {
                info.AppendLine("<li>Заявок пока нет</li>");
            }
            info.AppendLine("</ul>");

            // Активные товары
            info.AppendLine("<h3>📦 Активные товары:</h3><ul>");
            var activeProducts = await _db.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Take(10)
                .ToListAsync();

            foreach (var prod in activeProducts)
            {
                info.AppendLine($"<li>{prod.Name} - {prod.Price:N2} ₽/{prod.Unit}</li>");
            }
            info.AppendLine("</ul>");

            info.AppendLine("<p><a href='/'>← На главную</a> | <a href='/Requests'>← К заявкам</a></p>");
            info.AppendLine("</body></html>");

            return Content(info.ToString(), "text/html; charset=utf-8");
        }
    }
}
