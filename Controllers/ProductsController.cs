using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerRequestPortal.Models;
using CustomerRequestPortal.Data;

namespace CustomerRequestPortal.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Products/Catalog
        public async Task<IActionResult> Catalog(string? search, string? category, string sort = "name")
        {
            var products = _db.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchText = search.Trim();
                products = products.Where(p =>
                    p.Name.Contains(searchText) ||
                    (p.Description != null && p.Description.Contains(searchText)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p => p.Category == category);
            }

            products = sort switch
            {
                "price_desc" => products.OrderByDescending(p => p.Price),
                "price_asc" => products.OrderBy(p => p.Price),
                "stock_desc" => products.OrderByDescending(p => p.StockQuantity),
                "category" => products.OrderBy(p => p.Category).ThenBy(p => p.Name),
                _ => products.OrderBy(p => p.Name)
            };

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Sort = sort;
            ViewBag.Categories = await _db.Products
                .Where(p => p.IsActive && p.Category != null && p.Category != "")
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(await products.ToListAsync());
        }

        // GET: Products
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> Index(string? search, string? category, string? availability, string sort = "name")
        {
            var products = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchText = search.Trim();
                products = products.Where(p =>
                    p.Name.Contains(searchText) ||
                    (p.Description != null && p.Description.Contains(searchText)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p => p.Category == category);
            }

            products = availability switch
            {
                "active" => products.Where(p => p.IsActive),
                "inactive" => products.Where(p => !p.IsActive),
                "in_stock" => products.Where(p => p.StockQuantity > 0),
                "low_stock" => products.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 100),
                "out_of_stock" => products.Where(p => p.StockQuantity <= 0),
                _ => products
            };

            products = sort switch
            {
                "price_desc" => products.OrderByDescending(p => p.Price),
                "price_asc" => products.OrderBy(p => p.Price),
                "stock_desc" => products.OrderByDescending(p => p.StockQuantity),
                "stock_asc" => products.OrderBy(p => p.StockQuantity),
                "category" => products.OrderBy(p => p.Category).ThenBy(p => p.Name),
                _ => products.OrderBy(p => p.Name)
            };

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Availability = availability;
            ViewBag.Sort = sort;
            ViewBag.Categories = await _db.Products
                .Where(p => p.Category != null && p.Category != "")
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            ViewBag.TotalProducts = await products.CountAsync();
            ViewBag.ActiveProducts = await _db.Products.CountAsync(p => p.IsActive);
            ViewBag.LowStockProducts = await _db.Products.CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= 100);
            ViewBag.OutOfStockProducts = await _db.Products.CountAsync(p => p.StockQuantity <= 0);

            return View(await products.ToListAsync());
        }

        // GET: Products/Create
        [Authorize(Roles = AppRoles.Administrator)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                product.IsActive = true;
                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                TempData["Success"] = $"✅ Товар \"{product.Name}\" успешно добавлен!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _db.Products.FindAsync(id);
                    if (existingProduct == null) return NotFound();

                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Unit = product.Unit;
                    existingProduct.Category = product.Category;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.IsActive = product.IsActive;

                    _db.Update(existingProduct);
                    await _db.SaveChangesAsync();

                    TempData["Success"] = $"✅ Товар \"{product.Name}\" успешно обновлён!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                var productName = product.Name;
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();

                TempData["Success"] = $"✅ Товар \"{productName}\" успешно удалён!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/5 (подтверждение)
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        private bool ProductExists(int id)
        {
            return _db.Products.Any(e => e.Id == id);
        }
    }
}
