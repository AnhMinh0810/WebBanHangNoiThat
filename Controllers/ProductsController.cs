using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using furniture_store.Models;
using Microsoft.AspNetCore.Authorization;

namespace furniture_store.Controllers
{
    public class ProductsController : Controller
    {
        private readonly FurnitureContext _context;

        public ProductsController(FurnitureContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string? category, string? sortOrder, int page = 1)
        {
            if (page < 1) page = 1;

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            ViewData["CurrentCategory"] = category;
            ViewData["CurrentSort"] = sortOrder;

            IQueryable<Product> query = _context.Products.Include(p => p.Category);
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category != null && p.Category.Slug == category);
            }

            int totalItems = await query.CountAsync();
            int pageSize = 8;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page > totalPages && totalPages > 0)
            {
                page = totalPages;
            }

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;

            if (sortOrder == "price_asc")
            {
                query = query.OrderBy(p => p.Price);
            }
            else if (sortOrder == "price_desc")
            {
                query = query.OrderByDescending(p => p.Price);
            }
            else
            {
                query = query.OrderBy(p => p.Id);
            }

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(products);
        }

        // GET: Products/Admin
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Admin()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // GET: Products/Revenue
        [Authorize(Roles = "admin")]
        public IActionResult Revenue()
        {
            return View();
        }

        // GET: Products/GetRevenueData
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetRevenueData(string? date)
        {
            DateTime selectedDate;
            if (string.IsNullOrEmpty(date) || !DateTime.TryParse(date, out selectedDate) || selectedDate.Year < 1754)
            {
                selectedDate = DateTime.Today;
            }

            var startOfDay = selectedDate.Date;
            var endOfDay = startOfDay.AddDays(1);
            decimal dailyRevenue = await _context.Orders
                .Where(o => o.Status == "paid" && o.CreatedAt != null && o.CreatedAt.Value >= startOfDay && o.CreatedAt.Value < endOfDay)
                .SumAsync(o => o.Total);

            var startOfMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            decimal monthlyRevenue = await _context.Orders
                .Where(o => o.Status == "paid" && o.CreatedAt != null && o.CreatedAt.Value >= startOfMonth && o.CreatedAt.Value < endOfMonth)
                .SumAsync(o => o.Total);

            var startOfYear = new DateTime(selectedDate.Year, 1, 1);
            var endOfYear = startOfYear.AddYears(1);
            decimal yearlyRevenue = await _context.Orders
                .Where(o => o.Status == "paid" && o.CreatedAt != null && o.CreatedAt.Value >= startOfYear && o.CreatedAt.Value < endOfYear)
                .SumAsync(o => o.Total);

            var startDate = selectedDate.Date.AddDays(-29);
            var endDate = selectedDate.Date.AddDays(1); // exclusive

            var ordersInPeriod = await _context.Orders
                .Where(o => o.Status == "paid" && o.CreatedAt != null && o.CreatedAt.Value >= startDate && o.CreatedAt.Value < endDate)
                .Select(o => new { CreatedAt = o.CreatedAt!.Value, o.Total })
                .ToListAsync();

            var dates = Enumerable.Range(-29, 30).Select(i => selectedDate.Date.AddDays(i)).ToList();
            var chartData = dates.Select(d => new
            {
                dateLabel = d.ToString("dd"),
                fullDate = d.ToString("yyyy-MM-dd"),
                total = ordersInPeriod.Where(o => o.CreatedAt.Date == d.Date).Sum(o => o.Total)
            }).ToList();

            return Json(new
            {
                success = true,
                selectedDate = selectedDate.ToString("yyyy-MM-dd"),
                dailyRevenue,
                monthlyRevenue,
                yearlyRevenue,
                chartData
            });
        }

        // GET: Products/Orders
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View(orders);
        }

        // POST: Products/UpdateOrderStatus
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            var allowedStatuses = new[] { "pending", "shipped", "paid" };
            if (!allowedStatuses.Contains(status))
            {
                return Json(new { success = false, message = "Trạng thái không hợp lệ." });
            }

            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, newStatus = status });
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Slug,Description,Price,ImageUrl,Stock,CategoryId,Featured,CreatedAt")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Slug,Description,Price,ImageUrl,Stock,CategoryId,Featured,CreatedAt")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
