using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myhijyenProje.Data;

namespace myhijyenProje.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard(string range = "monthly", DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime today = DateTime.Today;
            DateTime start;
            DateTime end;

            switch (range)
            {
                case "daily":
                    start = today;
                    end = today.AddDays(1);
                    break;

                case "weekly":
                    start = today.AddDays(-6);
                    end = today.AddDays(1);
                    break;

                case "yearly":
                    start = new DateTime(today.Year, 1, 1);
                    end = today.AddDays(1);
                    break;

                case "custom":
                    start = startDate ?? today;
                    end = (endDate ?? today).AddDays(1);
                    break;

                case "monthly":
                default:
                    start = new DateTime(today.Year, today.Month, 1);
                    end = today.AddDays(1);
                    break;
            }

            var filteredOrders = _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end);

            ViewBag.Range = range;
            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.AddDays(-1).ToString("yyyy-MM-dd");

            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalOrders = await filteredOrders.CountAsync();

            ViewBag.TotalSales = await filteredOrders
                .SumAsync(o => (decimal?)o.Total) ?? 0;

            ViewBag.LowStockProducts = await _context.Products
                .Where(p => p.Stock <= 5)
                .OrderBy(p => p.Stock)
                .Take(8)
                .ToListAsync();

            ViewBag.LastOrders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(8)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}